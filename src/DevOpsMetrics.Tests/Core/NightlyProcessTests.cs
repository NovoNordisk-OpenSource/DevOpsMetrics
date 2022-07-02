﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DevOpsMetrics.Core.DataAccess.TableStorage;
using DevOpsMetrics.Core.Models.Common;
using DevOpsMetrics.Core.Models.GitHub;
using DevOpsMetrics.Function;
using DevOpsMetrics.Service.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOpsMetrics.Tests.Core
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestCategory("L1Test")]
    [TestClass]
    public class NightlyProcessTests : BaseConfiguration
    {
        [TestMethod]
        public async Task GitHubProcessingTest()
        {
            //Arrange
            TableStorageConfiguration tableStorageConfig = Common.GenerateTableAuthorization(base.Configuration);
            ProcessingResult result = null;
            AzureTableStorageDA da = new();
            BuildsController buildsController = new(base.Configuration, da);
            PullRequestsController pullRequestsController = new(base.Configuration, da);
            SettingsController settingsController = new(base.Configuration, da);
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());
            ILogger log = loggerFactory.CreateLogger<NightlyProcessTests>();

            //Act
            List<GitHubSettings> results = da.GetGitHubSettingsFromStorage(tableStorageConfig, tableStorageConfig.TableGitHubSettings, null);
            foreach (GitHubSettings item in results)
            {
                if (item.Repo == "AzurePipelinesToGitHubActionsConverterWeb")
                {
                    result = await Processing.ProcessGitHubItem(item, 30, 20,
                        buildsController, pullRequestsController, settingsController,
                        log, 0);
                }
            }

            //Assert
            Assert.IsNotNull(result);
        }



        //[TestMethod]
        //public async Task GHUpdateAzurePipelinesToGitHubActionsConverterWebSettingDAIntegrationTest()
        //{
        //    //Arrange
        //    TableStorageConfiguration tableStorageConfig = Common.GenerateTableAuthorization(base.Configuration);
        //    string owner = "samsmithnz";
        //    string repo = "AzurePipelinesToGitHubActionsConverterWeb";
        //    string branch = "main";
        //    string workflowName = "Pipelines to Actions website CI/CD";
        //    string workflowId = "43084";
        //    string resourceGroupName = "PipelinesToActions";
        //    int itemOrder = 3;
        //    bool showSetting = true;

        //    //Act
        //    AzureTableStorageDA da = new();
        //    bool result = await da.UpdateGitHubSettingInStorage(tableStorageConfig, tableStorageConfig.TableGitHubSettings,
        //            owner, repo, branch, workflowName, workflowId, resourceGroupName, itemOrder, showSetting);

        //    //Assert
        //    Assert.IsTrue(result == true);
        //}


        [TestMethod]
        public async Task GHUpdateAllDevOpsMetricsIntegrationTest()
        {
            //Arrange
            TableStorageConfiguration tableStorageConfig = Common.GenerateTableAuthorization(Configuration);
            int numberOfDays = 30;
            int maxNumberOfItems = 20;
            int totalResults = 0;
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole());
            ILogger log = loggerFactory.CreateLogger<NightlyProcessTests>();

            //Act
            AzureTableStorageDA azureTableStorageDA = new();
            BuildsController buildsController = new(Configuration, azureTableStorageDA);
            PullRequestsController pullRequestsController = new(Configuration, azureTableStorageDA);
            SettingsController settingsController = new(Configuration, azureTableStorageDA);
            List<GitHubSettings> ghSettings = settingsController.GetGitHubSettings();

            foreach (GitHubSettings item in ghSettings)
            {
                ProcessingResult ghResult = await Processing.ProcessGitHubItem(item, numberOfDays, maxNumberOfItems,
                               buildsController, pullRequestsController, settingsController, log, totalResults);
                totalResults = ghResult.TotalResults;
            }

            //Assert
            Assert.IsTrue(totalResults >= 0);
        }

    }
}