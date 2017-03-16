define(["handlebars.min", "common", "text!../../handlebars/template.html"], function (Handlebars, common, html_template) {
    var sysmenus;//系统菜单
    var labmenus;//标签菜单显示
    var urls;//模板数据请求地址
    var templates;//模板内容
    urls = new Array();
    templates = new Array();//handlebars模板对象
    labmenus = new Array();

    //初始化
    function init () {
        common.validateuser();//身份验证
        loadsysmenus();
        loadrouter();
    }

   
    //加载路由
    function loadrouter() {
        //菜单通过路由方式打开页面
        var openlabel = function (menuId) {
            //console.log("openmenu:" + menuId);
            if (menuId == 'home') {
                $('#content_label').html('<strong class="am-text-primary am-text-lg">首页</strong> /');
            } else if (menuId == 'quit') {
                //console.log(menuId);
            } else {
                $('#content_label').html('<strong class="am-text-primary am-text-lg">' + labmenus[menuId][0] + '</strong> / <small>' + labmenus[menuId][1] + '</small>');
            }
        };

        var opencontent = function (menuId) {
            //console.log("loadcontent:" + menuId);
            if (menuId == 'home') {

            } else if (menuId == 'quit') {
                if ($.cookie("token")) {
                    $.cookie("token", null);//注销就删除cookie
                    window.location.href = 'login.html';
                }
            } else {
                showpage(menuId);
            }
        };

        var routes = {
            '/openmenu/:menuId': [openlabel, opencontent]
        };
        var router = Router(routes);
        router.init();
    }

    //加载系统菜单
    function loadsysmenus() {
        //系统菜单Json对象
        sysmenus = [
            {
                "moudleid": "node", "moudlename": "节点配置管理", "child": [
                { "Id": "debug", "Name": "日志输出" },
                { "Id": "clientlist", "Name": "客户端列表" },
                { "Id": "showmnodeconfig", "Name": "配置信息" },
                { "Id": "sevicelist", "Name": "本地有效插件" },
                { "Id": "testsevice", "Name": "测试插件服务" },
                { "Id": "task", "Name": "定时任务" },
                { "Id": "register", "Name": "注册中间件" },
                { "Id": "localcmd", "Name": "执行命令" }
                ]
            }];
        if ($.cookie("token"))
        {
            sysmenus=$.merge([{
                "moudleid": "root", "moudlename": "中心监控平台", "child": [
                { "Id": "mnodelist", "Name": "中间件管理" },
                { "Id": "pluginlist", "Name": "服务插件管理" },
                { "Id": "mnodeplugin", "Name": "节点配置插件" },
                { "Id": "mnodestate", "Name": "节点监控图" },
                { "Id": "upgrade", "Name": "发布升级包" }
                //{ "Id": "testremoteservice", "Name": "远程测试服务" },
                //{ "Id": "remotecmd", "Name": "远程执行命令" }
                ]
            }], sysmenus);
        }
       
        $.each(sysmenus, function (i, n) {
            $.each(n.child, function (k, m) {
                labmenus[m.Id] = [n.moudlename, m.Name];
            });
        });

        $('#content_body').html(html_template);//加载html模板文本

        //显示系统菜单
        var menu_tpl = Handlebars.compile($("#menu-template").html());
        var menu_html = menu_tpl(sysmenus);
        $('#sysmenus').html(menu_html);

        $('#content_body').html("");//清空
    }

    //动态加载页面
    function showpage(menuId) {
        $('#content_body').html("");//先清空
        switch (menuId) {
            case "debug":
                require(["../../handlebars/debuglog"], function (page) {
                    page.showpage(menuId, urls, templates);
                });
                break;
            default:
                require(["../../handlebars/"+menuId], function (page) {
                    page.showpage(menuId, urls, templates);
                });
                break;
        }
    }



    //通用
    function show_common(menuId, para, callback, errorcallback) {
        if (!urls[menuId] || !templates[menuId]) {
            $('#content_body').html(html_template);//加载html模板文本
            //设置多个url和模板
            urls[menuId] = para;

            //时间格式化
            Handlebars.registerHelper("todate", function (value) {
                return $.formatDateTime('yy-mm-dd g:ii:ss', new Date(value));
            });
            templates[menuId] = Handlebars.compile($("#" + menuId + "-template").html());
        }

        common.simpleAjax(urls[menuId], {}, function (data) {
            var context = { data: common.toJson(data) };
            var html = templates[menuId](context);
            $('#content_body').html(html);

            if (callback) {
                callback(data);
            }
        }, errorcallback);
    }

    //默认页面
    function show_default(menuId, para) {
        
        show_common(menuId, para, null, function () {
            var context = { data: [] };
            var html = templates[menuId](context);
            $('#content_body').html(html);
        });
    }

    return {
        init: init
    };
});