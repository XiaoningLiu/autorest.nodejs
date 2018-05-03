// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// 

using AutoRest.Core;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.NodeJS.Model;
using AutoRest.NodeJS.vanilla.Templates;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AutoRest.Core.Utilities.DependencyInjection;

namespace AutoRest.NodeJS
{
    public class CodeGeneratorJs : CodeGenerator
    {
        private const string ClientRuntimePackage = "ms-rest version 2.0.0";

        public override string ImplementationFileExtension => ".js";

        public override string UsageInstructions => $"The {ClientRuntimePackage} or higher npm package is required to execute the generated code.";

        /// <summary>
        ///     Generate NodeJS client code 
        /// </summary>
        /// <param name="serviceClient"></param>
        /// <returns></returns>
        public override async Task Generate(CodeModel cm)
        {
            GeneratorSettingsJs generatorSettings = Singleton<GeneratorSettingsJs>.Instance;

            var codeModel = cm as CodeModelJs;
            if (codeModel == null)
            {
                throw new InvalidCastException("CodeModel is not a NodeJS code model.");
            }

            codeModel.PopulateFromSettings(generatorSettings);

            // Service client
            // Generate Index.js entry file for the server.
            await GenerateServiceClientJs(() => new ServiceClientTemplate { Model = codeModel }, generatorSettings).ConfigureAwait(false);

            // Models
            // Not Change from the Node Js Client
            if (codeModel.ModelTypes.Any())
            {
                await GenerateModelIndexJs(() => new ModelIndexTemplate { Model = codeModel }, generatorSettings).ConfigureAwait(false);

                await GenerateModelIndexDts(() => new ModelIndexTemplateTS { Model = codeModel }, generatorSettings).ConfigureAwait(false);

                foreach (CompositeTypeJs modelType in codeModel.ModelTemplateModels)
                {
                    await GenerateModelJs(modelType, generatorSettings).ConfigureAwait(false);
                }
            }

            // MethodGroups
            // Generate Router (Name not change for convinent)
            if (codeModel.MethodGroupModels.Any())
            {
                await GenerateRouteEntryTemplateJs(codeModel, generatorSettings).ConfigureAwait(false);

                foreach (MethodGroupJs methodGroupModel in codeModel.MethodGroupModels)
                {
                    await GenerateRouteGroupJs(() => new RouteGroupTemplate { Model = methodGroupModel }, generatorSettings).ConfigureAwait(false);
                }
            }

            // Generate Actions
            if (codeModel.MethodGroupModels.Any())
            {
                await GenerateActionEntryTemplateJs(codeModel, generatorSettings).ConfigureAwait(false);

                foreach (MethodGroupJs methodGroupModel in codeModel.MethodGroupModels)
                {
                    foreach (MethodJs method in methodGroupModel.MethodTemplateModels)
                    {
                        await GenerateActionJs(() => new ActionTemplate { Model = method }, generatorSettings).ConfigureAwait(false);
                    }
                }
            }

            await GenerateConstantsTemplateJs(codeModel, generatorSettings).ConfigureAwait(false);

            await GenerateUtilTemplateJs(codeModel, generatorSettings).ConfigureAwait(false);

            await GeneratePackageJson(codeModel, generatorSettings).ConfigureAwait(false);

            await GenerateReadmeMd(() => new ReadmeTemplate { Model = codeModel }, generatorSettings).ConfigureAwait(false);

            await GenerateLicenseTxt(codeModel, generatorSettings).ConfigureAwait(false);
        }

        protected async Task GenerateServiceClientJs<T>(Func<Template<T>> serviceClientTemplateCreator, GeneratorSettingsJs generatorSettings) where T : CodeModelJs
        {
            Template<T> serviceClientTemplate = serviceClientTemplateCreator();
            await Write(serviceClientTemplate, "index.js");
        }

        protected async Task GenerateModelIndexJs<T>(Func<Template<T>> modelIndexTemplateCreator, GeneratorSettingsJs generatorSettings) where T : CodeModelJs
        {
            Template<T> modelIndexTemplate = modelIndexTemplateCreator();
            await Write(modelIndexTemplate, GetModelSourceCodeFilePath(generatorSettings, "index.js")).ConfigureAwait(false);
        }

        protected async Task GenerateModelIndexDts<T>(Func<Template<T>> modelIndexTemplateCreator, GeneratorSettingsJs generatorSettings) where T : CodeModelJs
        {
            Template<T> modelIndexTemplateTS = modelIndexTemplateCreator();
            await Write(modelIndexTemplateTS, GetModelSourceCodeFilePath(generatorSettings, "index.d.ts")).ConfigureAwait(false);
        }

        protected async Task GenerateModelJs(CompositeTypeJs model, GeneratorSettingsJs generatorSettings)
        {
            var modelTemplate = new ModelTemplate { Model = model };
            await Write(modelTemplate, GetModelSourceCodeFilePath(generatorSettings, model.NameAsFileName.ToCamelCase() + ".js")).ConfigureAwait(false);
        }

        protected async Task GenerateConstantsTemplateJs(CodeModelJs codeModel, GeneratorSettingsJs generatorSettings)
        {
            var ConstantsTemplate = new ConstantsTemplate {Model = codeModel};
            await Write(ConstantsTemplate, GetCoreSourceCodeFilePath(generatorSettings, "constants.js"));
        }

        protected async Task GenerateUtilTemplateJs(CodeModelJs codeModel, GeneratorSettingsJs generatorSettings)
        {
            var UtilTemplate = new UtilTemplate {Model = codeModel};
            await Write(UtilTemplate, GetCoreSourceCodeFilePath(generatorSettings, "util.js"));
        }

        protected async Task GenerateRouteEntryTemplateJs(CodeModelJs codeModel, GeneratorSettingsJs generatorSettings)
        {
            var RouteEntryTemplate = new RouteEntryTemplate { Model = codeModel };
            await Write(RouteEntryTemplate, GetSourceCodeFilePath(generatorSettings, "route.js")).ConfigureAwait(false);
        }

        protected async Task GenerateActionEntryTemplateJs(CodeModelJs codeModel, GeneratorSettingsJs generatorSettings)
        {
            var ActionEntryTemplate = new ActionEntryTemplate { Model = codeModel };
            await Write(ActionEntryTemplate, GetSourceCodeFilePath(generatorSettings, "action.js")).ConfigureAwait(false);
        }


        protected async Task GenerateRouteGroupJs<T>(Func<Template<T>> routeGroupTemplateCreator, GeneratorSettingsJs generatorSettings) where T : MethodGroupJs
        {
            Template<T> routeGroupTemplate = routeGroupTemplateCreator();
            await Write(routeGroupTemplate, GetRouteSourceCodeFilePath(generatorSettings, routeGroupTemplate.Model.TypeName.ToCamelCase() + ".js")).ConfigureAwait(false);
        }

        protected async Task GenerateActionJs<T>(Func<Template<T>> actionTemplateCreator, GeneratorSettingsJs generatorSettings) where T : MethodJs
        {
            Template<T> actionTemplate = actionTemplateCreator();
            await Write(actionTemplate, GetActionSourceCodeFilePath(generatorSettings, actionTemplate.Model.Group.ToCamelCase(), actionTemplate.Model.SerializedName.ToPascalCase() + ".js")).ConfigureAwait(false);
        }

        protected async Task GeneratePackageJson(CodeModelJs codeModel, GeneratorSettingsJs generatorSettings)
        {
            if (generatorSettings.GeneratePackageJson)
            {
                var packageJson = new PackageJson { Model = codeModel };
                await Write(packageJson, "package.json").ConfigureAwait(false);
            }
        }

        protected async Task GenerateReadmeMd<T>(Func<Template<T>> readmeMdTemplateCreator, GeneratorSettingsJs generatorSettings) where T : CodeModelJs
        {
            if (generatorSettings.GenerateReadmeMd)
            {
                Template<T> readmeTemplate = readmeMdTemplateCreator();
                await Write(readmeTemplate, "README.md").ConfigureAwait(false);
            }
        }

        protected async Task GenerateLicenseTxt(CodeModelJs codeModel, GeneratorSettingsJs generatorSettings)
        {
            if (generatorSettings.GenerateLicenseTxt)
            {
                LicenseTemplate license = new LicenseTemplate { Model = codeModel };
                await Write(license, "LICENSE.txt").ConfigureAwait(false);
            }
        }

        protected string GetModelSourceCodeFilePath(GeneratorSettingsJs generatorSettings, string modelFileName)
            => GetSourceCodeFilePath(generatorSettings, "models", modelFileName);

        protected string GetRouteSourceCodeFilePath(GeneratorSettingsJs generatorSettings, string routeFileName)
            => GetSourceCodeFilePath(generatorSettings, "routes", routeFileName);

        protected string GetActionSourceCodeFilePath(GeneratorSettingsJs generatorSettings, string methodGroupName, string actionFileName)
            =>GetSourceCodeFilePath(generatorSettings, "actions",methodGroupName , actionFileName);

        protected string GetCoreSourceCodeFilePath(GeneratorSettingsJs generatorSettings, string coreFileName)
            => GetSourceCodeFilePath(generatorSettings, "core", coreFileName);

        protected string GetSourceCodeFilePath(GeneratorSettingsJs generatorSettings, params string[] pathSegments)
        {
            string[] totalPathSegments = new string[pathSegments.Length + 1];
            totalPathSegments[0] = generatorSettings.SourceCodeFolderPath;
            for (int i = 0; i < pathSegments.Length; i++)
            {
                totalPathSegments[1 + i] = pathSegments[i];
            }
            return Path.Combine(totalPathSegments);
        }
    }
}
