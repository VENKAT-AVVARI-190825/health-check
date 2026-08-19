pipeline {
    agent any

    environment {
        AWS_REGION = "${env.AWS_REGION ?: 'eu-west-1'}"
        ECR_REGISTRY = "${env.ECR_REGISTRY}"
        ECR_REPOSITORY_API = "${env.ECR_REPOSITORY_API ?: 'health-check-api'}"
        ECR_REPOSITORY_FRONTEND = "${env.ECR_REPOSITORY_FRONTEND ?: 'health-check-frontend'}"
        IMAGE_TAG = "${env.BUILD_NUMBER}"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Configure AWS') {
            steps {
                withCredentials([[$class: 'AmazonWebServicesCredentialsBinding', credentialsId: 'aws-credentials']]) {
                    sh 'aws sts get-caller-identity'
                }
            }
        }

        stage('Deploy API to ECS') {
            steps {
                withCredentials([[$class: 'AmazonWebServicesCredentialsBinding', credentialsId: 'aws-credentials']]) {
                    sh '''
                        aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$ECR_REGISTRY"
                        aws ecs update-service \
                          --cluster health-check-cluster \
                          --service health-check-api-service \
                          --force-new-deployment
                    '''
                }
            }
        }

        stage('Deploy Frontend to ECS') {
            steps {
                withCredentials([[$class: 'AmazonWebServicesCredentialsBinding', credentialsId: 'aws-credentials']]) {
                    sh '''
                        aws ecs update-service \
                          --cluster health-check-cluster \
                          --service health-check-frontend-service \
                          --force-new-deployment
                    '''
                }
            }
        }
    }
}
