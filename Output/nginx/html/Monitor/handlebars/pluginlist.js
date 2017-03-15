define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/pluginlist.html", "jquery.json"], function ($, common, Handlebars, html_template) {

    //通用
    function show_common(menuId, para, urls, templates, callback, errorcallback) {
        if (!urls[menuId] || !templates[menuId]) {
            $('#content_body').html(html_template);//加载html模板文本
            //设置多个url和模板
            urls[menuId] =  para;

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

    //保存插件
    function saveplugin(menuId, urls, templates, formdata) {
        var tooltip = $('#vld-tooltip').hide();
        if (formdata) {
            $('#txt_pluginname').val(formdata.pluginname);
            $('#txt_title').val(formdata.title);
            $('#txt_versions').val(formdata.versions);
            $('#txt_author').val(formdata.author);
            $('#txt_introduce').val(formdata.introduce);
        } else {
            $('#txt_pluginname').val("");
            $('#txt_title').val("");
            $('#txt_versions').val("");
            $('#txt_author').val("");
            $('#txt_introduce').val("");
            formdata = {
                pluginname: '',
                title: '',
                versions: '',
                author: '',
                introduce:''
            };
        }

        $('#plugin_modal').modal({
            relatedTarget: this,
            closeOnConfirm: false,
            //closeViaDimmer:false,
            //dimmer:false,
            onConfirm: function (e) {
                tooltip.hide();
                if ($('#txt_pluginname').val() == "") {
                    tooltip.text("插件名不能为空！").show();
                    return;
                }
                if ($('#txt_title').val() == "") {
                    tooltip.text("名称不能为空！").show();
                    return;
                }
                if ($('#txt_versions').val() == "") {
                    tooltip.text("版本不能为空！").show();
                    return;
                }
                if ($('#txt_author').val() == "") {
                    tooltip.text("作者不能为空！").show();
                    return;
                }
                formdata.pluginname = $('#txt_pluginname').val();
                formdata.title = $('#txt_title').val();
                formdata.versions = $('#txt_versions').val();
                formdata.author = $('#txt_author').val();
                formdata.introduce = $('#txt_introduce').val();
                common.simpleAjax("Monitor/SavePlugin", { para: $.toJSON(formdata) }, function (flag) {
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

    //显示中间件节点
    function showpage(menuId, urls, templates) {
        Handlebars.registerHelper("todelflag", function (value) {
            if (value == "0") {
                return "正常";
            } else {
                return "停用";
            }
        });
        Handlebars.registerHelper("tojson", function (value) {
            return $.toJSON(value);
        });
        show_common(menuId, "Monitor/GetPluginList", urls, templates, function () {
            $('#btn_plugin_ref').click(function () {
                showpage(menuId,urls,templates);
            });

            $('#btn_plugin_add').click(function () {
                saveplugin(menuId, urls, templates);
            });

            $('#btn_plugin_edit').click(function () {
                var value = $('#content_body table tbody tr.am-active').attr("value");
                value = $.evalJSON(value);
                saveplugin(menuId, urls, templates, value);
            });


            $('#btn_plugin_stop').click(function () {
                var value = $('#content_body table tbody tr.am-active').attr("value");
                value = $.evalJSON(value);
                if (value.id_string) {
                    var result = confirm('是否停用此插件服务？');
                    if (result) {
                        common.simpleAjax("Monitor/OnOffPlugin", { id: value.id_string }, function (flag) {
                            if (flag) {
                                showpage(menuId,urls,templates);
                            }
                        });
                    }
                }
            });

            $('#content_body table tbody tr').click(function () {
                $('#content_body table tbody tr').removeClass("am-active");
                $(this).addClass("am-active");
                var value = $('#content_body table tbody tr.am-active').attr("value");
                value = $.evalJSON(value);
                if (value.delflag == "1") {
                    $('#btn_plugin_stop').html("<span class='am-icon-twitch'></span> 启用");
                } else {
                    $('#btn_plugin_stop').html("<span class='am-icon-remove'></span> 停用");
                }
            });
        });
    }

    return {
        showpage: showpage
    };
});