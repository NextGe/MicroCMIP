define(["handlebars.min", "common", "text!../../handlebars/template.html"], function (Handlebars, common,html_template) {
    var sysmenus;//系统菜单
    var labmenus;//标签菜单显示
    var urls;//模板数据请求地址
    var templates;//模板内容
    urls = new Array();
    templates = new Array();//handlebars模板对象
    labmenus = new Array();
    //初始化
    $(document).ready(function () {
        //common.validateuser();//身份验证
        loadsysmenus();
        loadrouter();
    });

   
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
                "moudleid": "getstart", "moudlename": "开始使用", "child": [
                { "Id": "debug", "Name": "介绍" },
                { "Id": "clientlist", "Name": "下载" }
                ]
            }];
        sysmenus = $.merge(sysmenus,[{
            "moudleid": "user", "moudlename": "用户指南", "child": [
            { "Id": "mnodelist1", "Name": "中间件管理" }
            ]
        }]);
        sysmenus = $.merge(sysmenus,[{
            "moudleid": "develop", "moudlename": "开发者指南", "child": [
            { "Id": "mnodelist2", "Name": "中间件管理" }
            ]
        }]);
        sysmenus = $.merge(sysmenus,[{
            "moudleid": "manage", "moudlename": "管理员指南", "child": [
            { "Id": "mnodelist3", "Name": "中间件管理" }
            ]
        }]);


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

    //显示菜单页面
    function showpage(menuId) {
        $('#content_body').html("");//先清空
        switch (menuId) {
            case "clientlist":
                show_clientlist(menuId);
                break;
            case "debug":
                show_debuglog(menuId);
                break;
            case "showmnodeconfig":
                show_default(menuId,"mnodeconfig/showtext");
                break;
            case "sevicelist":
                show_default(menuId, "mnodeconfig/sevicelist");
                break;
            case "task":
                show_task(menuId);
                break;
            case "testsevice":
                show_testsevice(menuId);
                break;
            case "localcmd":
                show_localcmd(menuId);
                break;
            default:
                show_default(menuId);
                break;
        }
        
    }

});