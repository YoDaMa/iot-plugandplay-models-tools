﻿using Azure.Core.Diagnostics;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;

namespace Azure.Iot.ModelsRepository.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args) => await GetCommandLine().UseDefaults().Build().InvokeAsync(args);

        private static CommandLineBuilder GetCommandLine()
        {
            RootCommand root = new RootCommand("parent")
            {
                Description = $"Microsoft IoT Plug and Play Device Models Repository CLI v{Outputs.CliVersion}"
            };

            root.Add(BuildExportCommand());
            root.Add(BuildValidateCommand());
            root.Add(BuildImportModelCommand());

            root.AddGlobalOption(CommonOptions.Debug);
            root.AddGlobalOption(CommonOptions.Silent);

            CommandLineBuilder builder = new CommandLineBuilder(root);
            builder.UseMiddleware(async (context, next) =>
            {
                AzureEventSourceListener listener = null;
                try
                {
                    Outputs.WriteHeader();

                    if (context.ParseResult.Tokens.Any(x => x.Type == TokenType.Option && x.Value == "--debug"))
                    {
                        listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);
                        Outputs.WriteDebug(context.ParseResult.ToString());
                    }

                    if (context.ParseResult.Tokens.Any(x => x.Type == TokenType.Option && x.Value == "--silent"))
                    {
                        System.Console.SetOut(TextWriter.Null);
                    }

                    await next(context);
                }
                finally
                {
                    if (listener != null)
                    {
                        listener.Dispose();
                    }
                }
            });

            return builder;
        }

        private static Command BuildExportCommand()
        {
            var modelFileOpt = CommonOptions.ModelFile;
            modelFileOpt.IsHidden = true;

            Command exportModelCommand = new Command("export")
            {
                CommonOptions.Dtmi,
                modelFileOpt,
                CommonOptions.Repo,
                CommonOptions.Deps,
                CommonOptions.Output
            };

            exportModelCommand.Description =
                "Retrieve a model and its dependencies by dtmi using the target repository for model resolution.";
            exportModelCommand.Handler = CommandHandler.Create<string, FileInfo, string, DependencyResolutionOption, string>(Handlers.Export);

            return exportModelCommand;
        }

        private static Command BuildValidateCommand()
        {
            var modelFileOption = CommonOptions.ModelFile;
            modelFileOption.IsRequired = true; // Option is required for this command

            Command validateModelCommand = new Command("validate")
            {
                modelFileOption,
                CommonOptions.Repo,
                CommonOptions.Deps,
                CommonOptions.Strict,
            };

            validateModelCommand.Description =
                "Validates a model using the DTDL model parser & resolver. When validating a single model object " +
                "the target repository is used for model resolution. When validating an array of models only the array " +
                "contents is used for resolution.";

            validateModelCommand.Handler =
                CommandHandler.Create<FileInfo, string, DependencyResolutionOption, bool>(Handlers.Validate);

            return validateModelCommand;
        }

        private static Command BuildImportModelCommand()
        {
            var modelFileOption = CommonOptions.ModelFile;
            modelFileOption.IsRequired = true; // Option is required for this command
            var depsResOption = CommonOptions.Deps;
            depsResOption.IsHidden = true; // Option has limited value for this command

            Command importModelCommand = new Command("import")
            {
                modelFileOption,
                CommonOptions.LocalRepo,
                depsResOption,
                CommonOptions.Strict,
            };
            importModelCommand.Description =
                "Imports models from a model file into the local repository. The local repo is used for model resolution. ";
            importModelCommand.Handler = CommandHandler.Create<FileInfo, DirectoryInfo, DependencyResolutionOption, bool>(Handlers.Import);

            return importModelCommand;
        }
    }
}
