using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.Pipelines;
using Constructs;

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
                           
            }); ;
        }
    }
}
