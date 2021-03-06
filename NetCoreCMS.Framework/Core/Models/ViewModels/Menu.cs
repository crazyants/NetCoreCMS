﻿/*************************************************************
 *          Project: NetCoreCMS                              *
 *              Web: http://dotnetcorecms.org                *
 *           Author: OnnoRokom Software Ltd.                 *
 *          Website: www.onnorokomsoftware.com               *
 *            Email: info@onnorokomsoftware.com              *
 *        Copyright: OnnoRokom Software Ltd.                 *
 *          License: BSD-3-Clause                            *
 *************************************************************/

using System.Collections.Generic;

namespace NetCoreCMS.Framework.Core.Models.ViewModels
{
    public class Menu
    {
        public string ModuleId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string DisplayName { get; set; }
        public List<MenuItem> MenuItems { get; set; }
        public string IconCls { get; set; }
        public int Order { get; set; }
        public MenuType Type { get; set; }

        public enum MenuType
        {
            WebSite,
            Admin
        }
    }
}
