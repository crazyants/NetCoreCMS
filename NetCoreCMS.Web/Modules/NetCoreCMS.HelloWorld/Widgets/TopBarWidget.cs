﻿/*************************************************************
 *          Project: NetCoreCMS                              *
 *              Web: http://dotnetcorecms.org                *
 *           Author: OnnoRokom Software Ltd.                 *
 *          Website: www.onnorokomsoftware.com               *
 *            Email: info@onnorokomsoftware.com              *
 *        Copyright: OnnoRokom Software Ltd.                 *
 *          License: BSD-3-Clause                            *
 *************************************************************/

using NetCoreCMS.Framework.Core.Mvc.Views;
using NetCoreCMS.Framework.Core.Services;
using NetCoreCMS.Framework.Modules.Widgets;
using NetCoreCMS.HelloWorld.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreCMS.HelloWorld.Widgets
{
    public class TopBarWidget : Widget
    {
        IViewRenderService _viewRenderService;
        NccWebSiteWidgetService _websiteWidgetService;

        public TopBarWidget(
            IViewRenderService viewRenderService,
            NccWebSiteWidgetService websiteWidgetService):base("NetCoreCMS.Modules.Widgets.TopBarWidget", "TopBar", "TopBar Widget", "")
        {
            _viewRenderService = viewRenderService;
            _websiteWidgetService = websiteWidgetService;
        }

        public override void Init(long websiteWidgetId)
        {
            WebSiteWidgetId = websiteWidgetId;
            ViewFileName = "Widgets/TopBar";
            var webSiteWidget = _websiteWidgetService.Get(websiteWidgetId, true);
            if (webSiteWidget != null && !string.IsNullOrEmpty(webSiteWidget.WidgetConfigJson))
            {
                var configJson = webSiteWidget.WidgetConfigJson;
                var config = JsonConvert.DeserializeObject<dynamic>(configJson);
                DisplayTitle = config.title;
                Language = config.language;
                Footer = config.footer;
            }
        }

        public override string RenderBody()
        {
            var body = _viewRenderService.RenderToStringAsync<HelloWidgetController>(ViewFileName, null).Result;
            return body;
        }
    }
}
