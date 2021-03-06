﻿/*
 *  Dover Framework - OpenSource Development framework for SAP Business One
 *  Copyright (C) 2014  Eduardo Piva
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Contact me at <efpiva@gmail.com>
 * 
 */
using System;
using System.Reflection;
using Castle.Core.Logging;
using Dover.Framework.Interface;
using Dover.Framework.Service;

namespace Dover.Framework
{
    internal class Boot
    {
        public ILogger Logger { get; set; }

        private LicenseManager licenseManager;
        private IAddinLoader addinLoader;
        private IAddinManager addinManager;
        private IEventDispatcher dispatcher;
        private IFormEventHandler formEventHandler;
        private AddinAppEventHandler addinAppEventHandler;

        public Boot(LicenseManager licenseValidation, IAddinManager addinManager, IAddinLoader addinLoader,
            IEventDispatcher dispatcher, IFormEventHandler formEventHandler, I18NService i18nService,
            AddinAppEventHandler addinAppEventHandler)
        {
            this.licenseManager = licenseValidation;
            this.addinManager = addinManager;
            this.dispatcher = dispatcher;
            this.formEventHandler = formEventHandler;
            this.addinLoader = addinLoader;
            this.addinAppEventHandler = addinAppEventHandler;

            i18nService.ConfigureThreadI18n(System.Threading.Thread.CurrentThread);
        }

        internal void StartUp()
        {
            string moduleName = this.GetType().Assembly.GetName().Name;
            try
            {
                if (moduleName == "Framework")
                    moduleName = "Dover Framework";
                Logger.Info(String.Format(Messages.Starting, moduleName, this.GetType().Assembly.GetName().Version));
                var addins = licenseManager.ListAddins();
                dispatcher.RegisterEvents();
                StartFrameworkUI(); // load admin forms.
                addinManager.LoadAddins(addins);
                addinManager.Initialized = true;
                Logger.Info(String.Format(Messages.Started, moduleName, this.GetType().Assembly.GetName().Version));
            }
            catch (Exception e)
            {
                Logger.Fatal(string.Format(Messages.ErrorStartup, moduleName), e);
                Environment.Exit(10);
            }
        }

        private void StartFrameworkUI()
        {
            addinLoader.StartMenu(Assembly.GetExecutingAssembly());
            formEventHandler.RegisterForms();
        }

        internal bool StartThis()
        {
            string thisAsmName = (string)AppDomain.CurrentDomain.GetData("assemblyName");
            try
            {
                Assembly thisAsm = AppDomain.CurrentDomain.Load(thisAsmName);
                Logger.Info(String.Format(Messages.Starting, thisAsmName, thisAsm.GetName().Version));
                addinLoader.StartThis();
                dispatcher.RegisterEvents();
                formEventHandler.RegisterForms();
                addinAppEventHandler.RegisterEvents();
                Logger.Info(String.Format(Messages.Started, thisAsmName, thisAsm.GetName().Version));
                return true;
            }
            catch (Exception e)
            {
                Logger.Fatal(string.Format(Messages.ErrorStartup, thisAsmName), e);
                return false;
            }
        }
    }
}
