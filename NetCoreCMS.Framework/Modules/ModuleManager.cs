﻿/*
 * Author: Xonaki
 * Website: http://xonaki.com
 * Copyright (c) xonaki.com
 * License: BSD (3 Clause)
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using NetCoreCMS.Framework.Utility;
using Newtonsoft.Json;
using NetCoreCMS.Framework.Modules.Widgets;

namespace NetCoreCMS.Framework.Modules
{
    public class ModuleManager
    {
        List<IModule> modules = new List<IModule>();
        public List<IModule> LoadModules(IDirectoryContents moduleRootFolder)
        {   
            foreach (var moduleFolder in moduleRootFolder.Where(x => x.IsDirectory))
            {
                try
                {
                    var binFolder = new DirectoryInfo(Path.Combine(moduleFolder.PhysicalPath, "bin"));
                    if (!binFolder.Exists)
                    {
                        continue;
                    }

                    foreach (var file in binFolder.GetFileSystemInfos("*.dll", SearchOption.AllDirectories))
                    {
                        Assembly assembly;
                        try
                        {
                            assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file.FullName);
                        }
                        catch (FileLoadException ex )
                        {
                            continue;
                        }
                        catch(BadImageFormatException ex)
                        {
                            continue;
                        }

                        if (assembly.FullName.Contains(moduleFolder.Name))
                        {
                            modules.Add(new Module{ ModuleName = moduleFolder.Name, Assembly = assembly, Path = moduleFolder.PhysicalPath });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not load module from " + moduleFolder);
                }
            }
            return modules;
        }
        
        public List<IModule> RegisterModules(IMvcBuilder mvcBuilder, IServiceCollection services)
        {
            var instantiatedModuleList = new List<IModule>();

            foreach (var module in modules)
            {
                try
                {
                    // Register controller from modules
                    mvcBuilder.AddApplicationPart(module.Assembly);

                    // Register dependency in modules
                    var moduleInitializerType = module.Assembly.GetTypes().Where(x => typeof(IModule).IsAssignableFrom(x)).FirstOrDefault();
                    if (moduleInitializerType != null && moduleInitializerType != typeof(IModule))
                    {
                        var initilizedModule = (IModule)Activator.CreateInstance(moduleInitializerType);                        
                        initilizedModule.Init(services);                        
                        LoadModuleInfo(initilizedModule, module);
                        InitilizeWidgets(initilizedModule);
                        instantiatedModuleList.Add(initilizedModule);
                    }
                }
                catch (Exception ex)
                {
                    //RAISE GLOBAL ERROR
                }
            }
            
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ModuleViewLocationExpendar());
            });

            mvcBuilder.AddRazorOptions(o =>
            {
                foreach (var module in modules)
                {
                    o.AdditionalCompilationReferences.Add(MetadataReference.CreateFromFile(module.Assembly.Location));
                }
            });

            return instantiatedModuleList;
        }

        private void InitilizeWidgets(IModule module)
        {
            module.Widgets = new List<IWidget>();
            var widgetTypeList = module.Assembly.GetTypes().Where(x => x.GetInterfaces()?.Where(y => y.Name == typeof(IWidget).Name).FirstOrDefault() != null).ToList();
             
            foreach (var widgetType in widgetTypeList)
            {
                var widgetInstance = (IWidget)Activator.CreateInstance(widgetType);
                widgetInstance.Init();
                module.Widgets.Add(widgetInstance);

            }
             
        }
        public void LoadModuleInfo(IModule module, IModule moduleInfo)
        {
            var moduleConfigFile = Path.Combine(moduleInfo.Path, Constants.ModuleConfigFileName);
            if (File.Exists(moduleConfigFile))
            {                
                var moduleInfoFileJson = File.ReadAllText(moduleConfigFile);
                var loadedModule = JsonConvert.DeserializeObject<Module>(moduleInfoFileJson);
                module.AntiForgery = loadedModule.AntiForgery;
                module.Author = loadedModule.Author;
                module.Category = loadedModule.Category;
                module.Dependencies = loadedModule.Dependencies;
                module.Description = loadedModule.Description;
                module.ModuleId = loadedModule.ModuleId;
                
                module.ModuleTitle = loadedModule.ModuleTitle;
                module.NetCoreCMSVersion = loadedModule.NetCoreCMSVersion;
                module.SortName = loadedModule.SortName;
                module.Version = loadedModule.Version;
                module.Website = loadedModule.Website;

                module.ModuleName = moduleInfo.ModuleName;
                module.Assembly = moduleInfo.Assembly;
                module.Path = moduleInfo.Path;
            }
            else
            {
                //RAISE GLOBAL ERROR
            }
        }

    }
}
