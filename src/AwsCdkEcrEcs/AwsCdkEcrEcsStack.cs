using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.Pipelines;
using Constructs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.APIGateway;

namespace AwsCdkEcrEcs
{
    public class AwsCdkEcrEcsStack : Stack
    {
        internal AwsCdkEcrEcsStack(Construct scope, string id, IStackProps props = null) :
            base(scope, id, props)
        {
            // The code that defines your stack goes here
            Amazon.CDK.AWS.CodeCommit.IRepository repo =
                Amazon.CDK.AWS.CodeCommit.Repository.FromRepositoryName(this,
                    Constants.CODE_COMMIT_REPO_ID, Constants.CODE_COMMIT_REPO_NAME);

            var ecrRepo = Amazon.CDK.AWS.ECR.Repository.FromRepositoryName(this, 
                Constants.ECR_RepoName_ID,
                Constants.ECR_RepoName);

                Console.WriteLine($"ecrRepo: {ecrRepo.RepositoryUri}");

            CodePipeline pipeline = new CodePipeline(this, Constants.CODE_PIPELINE_ID,
                new CodePipelineProps
                {
                    PipelineName = Constants.CODE_PIPELINE,
                    Synth = new ShellStep("Synth", new ShellStepProps
                    {
                        Input = CodePipelineSource.CodeCommit(repo, Constants.CODE_COMMIT_BRANCH_NAME),
                        Commands = new string[] {
                                                "npm install -g aws-cdk",                                                
                                                "cdk synth RandomNameStack"
                                            }
                    })
                });

            var vpc = new Vpc(this, "random-name-vpc", new VpcProps
            {
               MaxAzs = 3, // Default is all AZs in region
               VpcName = "random-profile-vpc",
            });

            var cluster = new Cluster(this, "random-name-cluster", new ClusterProps
            {
                Vpc = vpc,
                ClusterName = "random-profile-cluster",
                
            });

            var taskDef = new FargateTaskDefinition(this, "random-name-taskdef", 
                new FargateTaskDefinitionProps {
                    MemoryLimitMiB = 512,                    
                    Cpu = 256,
                    ExecutionRole = Amazon.CDK.AWS.IAM.Role.FromRoleName(this, "random-name-ecs-ecr-role", "random-name-ecs-ecr-role"),
                    TaskRole = Amazon.CDK.AWS.IAM.Role.FromRoleName(this, "random-name-ecs-ecr-task-role", "random-name-ecs-ecr-role"),
                    
                });

            taskDef.AddContainer("random-name-taskDef", new ContainerDefinitionOptions {
                    Image = ContainerImage.FromRegistry($"{ecrRepo.RepositoryUri}:latest"), // ContainerImage.FromRegistry("amazon/amazon-ecs-sample")                    
                    ContainerName = "web",                                        
                    PortMappings = new PortMapping[] {
                        new PortMapping() { ContainerPort =  80 }
                    }
            });

             var ecs = new Amazon.CDK.AWS.ECS.Patterns.ApplicationLoadBalancedFargateService(this, "random-name-alb-ec2", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,                
                PublicLoadBalancer = true,
                TaskDefinition = taskDef,
                LoadBalancerName = "random-profile-alb",
                ServiceName = "random-profile-service",
                AssignPublicIp = true,
                           
            });

            taskDef.DefaultContainer.AddEnvironment("alb", ecs.LoadBalancer.LoadBalancerDnsName);


            // Give permission to S3.
			IBucket bucket = new Bucket(this, "profile-contacts", new BucketProps {
				BucketName = "profile-contacts"
			});

			// NOTE: For local testing, use 'Debug', otherwise use 'Release'.
			Function RandomSalaryGeneratorFunction = new Function(this, "RandomSalaryGenerator", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Code = Code.FromAsset("./src/Lambda/src/RandomSalaryGenerator/bin/RELEASE/net6.0/RandomSalaryGenerator.dll"),
                Handler = "Lambdas::Lambda::Run",
				Timeout = Duration.Seconds(15),
				MemorySize = 128
            });

			// Add environment property to Lambda.
			RandomSalaryGeneratorFunction.AddEnvironment("profile-contacts-bucket", "profile-contacts");
			
			bucket.GrantReadWrite(RandomSalaryGeneratorFunction);

			LambdaRestApi api = new LambdaRestApi(this, "RandomSalaryGeneratorAPI", 
                new LambdaRestApiProps
			{
				Handler = RandomSalaryGeneratorFunction,
				Proxy = false
			});
			

			var contactAPI = api.Root.AddResource("contact-api");
			
            LambdaIntegration lambdaProxyIntegrationRandomSalary = new LambdaIntegration(RandomSalaryGeneratorFunction);

            contactAPI.AddMethod("GET", lambdaProxyIntegrationRandomSalary);

        }
    }
}
