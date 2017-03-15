define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/mnodeplugin.html", "jquery.json", "amazeui.tree.min"], function ($, common, Handlebars, html_template) {

    //通用
    function show_common(menuId, para, urls, templates, callback, errorcallback) {
        if (!urls[menuId] || !templates[menuId]) {
            $('#content_body').html(html_template);//加载html模板文本
            //设置多个url和模板
            urls[menuId] = para;
            templates[menuId] = Handlebars.compile($("#" + menuId + "-template").html());
        }

        common.simpleAjax(urls[menuId], {}, function (data) {
            var context = { data: common.toJson(data) };
            var html = templates[menuId](context);
            $('#content_body').html(html);

            if (callback) {
                callback(common.toJson(data));
            }
        }, errorcallback);
    }

    function query(menuId, urls, templates) {
        var identify = $('#mnodelist').val();
        common.simpleAjax("Monitor/GetMNodePService", { identify: identify }, function (data) {
            //$('#firstTree').tree('closeAll');
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
            });
        });
    }

    function addlocal(menuId, urls, templates) {
        var tooltip = $('#vld-tooltip').hide();
        $('#addlocalplugin_modal').modal({
            relatedTarget: this,
            closeOnConfirm: false,
            //closeViaDimmer:false,
            //dimmer:false,
            onConfirm: function (e) {
                tooltip.hide();
                var selectdata = $('#txt_localplugin').val();
                if (selectdata == null) {
                    tooltip.text("请先选择一个插件！").show();
                    return;
                }

                var identify = $('#mnodelist').val();

                common.simpleAjax("Monitor/AddMNodePService", { identify: identify, type: 0, data: $.toJSON(selectdata) }, function (flag) {
                    if (flag) {
                        //$(this).modal('toggle');
                        $(this).modal('close');
                        $('.am-dimmer').hide();
                        showpage(menuId, urls, templates);
                    }
                });
            }
        });
    }

    function addremote(menuId, urls, templates) {
        var tooltip = $('#vld-tooltip2').hide();
        $('#addremoteplugin_modal').modal({
            relatedTarget: this,
            closeOnConfirm: false,
            //closeViaDimmer:false,
            //dimmer:false,
            onConfirm: function (e) {
                tooltip.hide();
                var selectplugin = $('#txt_remoteplugin').val();
                if (selectplugin == null) {
                    tooltip.text("请先选择一个插件！").show();
                    return;
                }
                var selectnode = $('#txt_remotemnode').val();
                if (selectnode == null) {
                    tooltip.text("请先选择一个节点！").show();
                    return;
                }
                var data = { pluginname: selectplugin, mnodeidentify: selectnode };
                var identify = $('#mnodelist').val();
                common.simpleAjax("Monitor/AddMNodePService", { identify: identify, type: 1, data: $.toJSON(data) }, function (flag) {
                    if (flag) {
                        //$(this).modal('toggle');
                        $(this).modal('close');
                        $('.am-dimmer').hide();
                        showpage(menuId, urls, templates);
                    }
                });
            }
        });
    }

    function delplugin(menuId, urls, templates) {
        if ($('body').data('type')) {
            var result = confirm('是否此节点的插件？');
            if (result) {
                var identify = $('body').data('mnodelist');
                var type = 0;
                if ($('body').data('type') == 'remoteplugin')
                    type = 1;
                var pluginname = $('body').data('value');
                common.simpleAjax("Monitor/DelMNodePService", { identify: identify, type: type, pluginname: pluginname }, function (flag) {
                    if (flag) {
                        showpage(menuId, urls, templates);
                    }
                });
            }
        }
    }

    //显示界面
    function showpage(menuId, urls, templates) {
        
        show_common(menuId, "Monitor/GetMNodePluginViewData?identify=0", urls, templates, function (data) {
            
            $('#txt_localplugin').selected();
            $('#txt_remoteplugin').selected();
            $('#txt_remotemnode').selected();

            if ($('body').data('mnodelist')) {
                $('#mnodelist').val($('body').data('mnodelist'));
            }
            $('#mnodelist').selected();

            $('#firstTree').tree({
                dataSource: function (options, callback) {
                    // 模拟异步加载
                    setTimeout(function () {
                        callback({ data: options.childs || data.tree });
                    }, 40);
                },
                multiSelect: false,
                cacheItems: true,
                folderSelect: false
            }).on('selected.tree.amui', function (e, selected) {
                //console.log('Select Event: ', selected);
                //console.log($('#firstTree').tree('selectedItems'));
                $('body').data('type',selected.target.attr.type);
                $('body').data('value', selected.target.attr.value);
            });

            $('#btn_query').click(function () {
                $('body').data('mnodelist', $('#mnodelist').val());
                //query(menuId, urls, templates);
                urls[menuId] = "Monitor/GetMNodePluginViewData?identify=" + $('#mnodelist').val();
                showpage(menuId, urls, templates);
            });

            $('#btn_plugin_addlocal').click(function () {
                addlocal(menuId, urls, templates);
            });

            $('#btn_plugin_addremote').click(function () {
                addremote(menuId, urls, templates);
            });

            $('#btn_plugin_del').click(function () {
                delplugin(menuId, urls, templates);
            });
        });
    }

    return {
        showpage: showpage
    };
});