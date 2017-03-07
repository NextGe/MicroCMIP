define(["handlebars.min", "common", "text!../../handlebars/template.html"], function (Handlebars, common, html_template) {
    var sysmenus;//系统菜单
    var labmenus;//标签菜单显示
    var urls;//模板数据请求地址
    var templates;//模板内容
    urls = new Array();
    templates = new Array();//handlebars模板对象
    labmenus = new Array();
    //初始化
    $(document).ready(function () {
        common.validateuser();//身份验证
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
                "moudleid": "node", "moudlename": "节点配置管理", "child": [
                { "Id": "debug", "Name": "日志输出" },
                { "Id": "clientlist", "Name": "客户端列表" },
                { "Id": "showmnodeconfig", "Name": "配置信息" },
                { "Id": "sevicelist", "Name": "本地插件服务" },
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
                { "Id": "mnodestate", "Name": "节点监控图" },
                { "Id": "upgrade", "Name": "发布升级包" },
                { "Id": "testremoteservice", "Name": "远程测试服务" },
                { "Id": "remotecmd", "Name": "远程执行命令" }
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

    //通用
    function show_common(menuId, para, callback, errorcallback) {
        if (!urls[menuId] || !templates[menuId]) {
            $('#content_body').html(html_template);//加载html模板文本
            //设置多个url和模板
            urls[menuId] = "http://127.0.0.1:8021/" + para;
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

    //客户端列表
    function show_clientlist(menuId) {
        show_common(menuId, "mnodeconfig/clientlist", function () {
            $('.btn-clientlist-refresh').click(function () {
                show_clientlist(menuId);
            });
        });
    }

    //日志
    function show_debuglog(menuId) {
        
        show_common(menuId, "mnodeconfig/debuglog?logtype=MidLog&date=", function () {
            $('#logdate').datepicker('setValue', new Date());
            if ($('body').data('logtype')) {
                $('#logtype').val($('body').data('logtype'));
                //$('#logtype').find('option[value="' + $('body').data('logtype') + '"]').attr('selected', true);
            }
            $('#logtype').selected();

            $('#btn_logsearch').click(function () {
                $('body').data('logtype', $('#logtype').val());
                urls[menuId] = "http://127.0.0.1:8021/mnodeconfig/debuglog?logtype=" + $('#logtype').val() + "&date=" + $('#logdate').data('date');
                show_debuglog(menuId);
            });
        });
    }

    //任务
    function show_task(menuId) {
        show_common(menuId, "mnodeconfig/GetTaskList", function () {
            
        });
    }

    //测试服务
    function show_testsevice(menuId) {

        show_common(menuId, "mnodeconfig/GetAllServices", function (data) {
            $('#firstTree').tree({
                dataSource: function (options, callback) {
                    // 模拟异步加载
                    setTimeout(function () {
                        callback({ data: options.childs || data });
                    }, 40);
                },
                multiSelect: false,
                cacheItems: true,
                folderSelect: false
            }).on('selected.tree.amui', function (e, selected) {
                //console.log('Select Event: ', selected);
                //console.log($('#firstTree').tree('selectedItems'));
                $('#txt_plugin').val(selected.target.attr.plugin);
                $('#txt_controller').val(selected.target.attr.controller);
                $('#txt_method').val(selected.target.attr.method);
            });

            $('#btn_request').click(function () {
                var para = { plugin: $('#txt_plugin').val(), controller: $('#txt_controller').val(), method: $('#txt_method').val(), para: $('#txt_parajson').val() };
                if (para.method && para.controller && para.plugin) {
                    common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/TestServices", para, function (data) {
                        $('#txt_responsejson').val(data);
                    });
                }
            });
        });
    }

    //执行命令
    function show_localcmd(menuId) {
        if (!urls[menuId] || !templates[menuId]) {
            $('#content_body').html(html_template);//加载html模板文本
            //设置多个url和模板
            urls[menuId] = "http://127.0.0.1:8021/";
            templates[menuId] = Handlebars.compile($("#" + menuId + "-template").html());
        }
        var context = { data: [] };
        var html = templates[menuId](context);
        $('#content_body').html(html);

        $('#btn_restart').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "quitall", arg: "" }, function (data) {
                    if (data) {
                        common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "startall", arg: "" });
                    }
                });
            }
        });

        $('#btn_restartbase').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartbase", arg: "" });
            }
        });

        $('#btn_restartroute').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartroute", arg: "" });
            }
        });

        $('#btn_restartwebapi').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartwebapi", arg: "" });
            }
        });

        $('#btn_restartmongodb').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartmongodb", arg: "" });
            }
        });

        $('#btn_restartnginx').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartnginx", arg: "" });
            }
        });

        $('#btn_exit').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("http://127.0.0.1:8021/mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "exit", arg: "" });
            }
        });
    }
});