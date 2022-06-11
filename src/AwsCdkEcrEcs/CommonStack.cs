using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace AwsCdkEcrEcs
{
    public class CommmonStack : Stack
    {
        internal CommmonStack(Construct scope, string id, IStackProps props = null) :
            base(scope, id, props)
        {
            // The code that defines your stack goes here
            var repo = new Amazon.CDK.AWS.CodeCommit.Repository(this,
                    Constants.CODE_COMMIT_REPO_ID, new Amazon.CDK.AWS.CodeCommit.RepositoryProps
                    {
                        RepositoryName = Constants.CODE_COMMIT_REPO_NAME
                    });

            var cdkRepo = new Amazon.CDK.AWS.CodeCommit.Repository(this,
                    Constants.CODE_COMMIT_CDK_REPO_ID, new Amazon.CDK.AWS.CodeCommit.RepositoryProps
                    {
                        RepositoryName = Constants.CODE_COMMIT_CDK_REPO_NAME,
                        Description = "aws-cdk-ecr-ecs cdk project",                        
                    });

            // var ecrRepo = new Amazon.CDK.AWS.ECR.Repository(this,
            //                 Constants.ECR_RepoName_ID, new Amazon.CDK.AWS.ECR.RepositoryProps() {
            //                     RepositoryName = Constants.ECR_RepoName
            //                 });

            var myCustomPolicy = new PolicyStatement(new PolicyStatementProps {
                Actions = new [] { 
                    "cloudwatch:*",
                    "ecs:*",
                    "ecr:*",
                    "imagebuilder:GetComponent",
                    "imagebuilder:GetContainerRecipe",
                    "ecr:GetAuthorizationToken",
                    "ecr:BatchGetImage",
                    "ecr:InitiateLayerUpload",
                    "ecr:UploadLayerPart",
                    "ecr:CompleteLayerUpload",
                    "ecr:BatchCheckLayerAvailability",
                    "ecr:GetDownloadUrlForLayer",
                    "ecr:PutImage",
                    "ec2:AuthorizeSecurityGroupIngress",
                    "ec2:Describe*",
                    "elasticloadbalancing:DeregisterInstancesFromLoadBalancer",
                    "elasticloadbalancing:DeregisterTargets",
                    "elasticloadbalancing:Describe*",
                    "elasticloadbalancing:RegisterInstancesWithLoadBalancer",
                    "elasticloadbalancing:RegisterTargets",
                    "ecr:GetAuthorizationToken",
                    "ecr:BatchCheckLayerAvailability",
                    "ecr:GetDownloadUrlForLayer",
                    "ecr:BatchGetImage",
                    "logs:CreateLogStream",
                    "logs:PutLogEvents" 
                },                
                Resources = new [] { "*" }
            }); 
    
            

            var ecsRole = new Amazon.CDK.AWS.IAM.Role(this, "random-name-ecs-ecr-role-id", new RoleProps
            {
                RoleName = "random-name-ecs-ecr-role",
                Description = "Role to run EC2 Instance and connect to ECR",
                AssumedBy = new CompositePrincipal(
                                new ServicePrincipal("ecs.amazonaws.com"),
                                new ServicePrincipal("ecs-tasks.amazonaws.com"))
                
            });

            ecsRole.AddToPolicy(myCustomPolicy);
        }
    }
}
