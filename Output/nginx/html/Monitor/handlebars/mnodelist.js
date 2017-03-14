define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/mnodelist.html", "jquery.json"], function ($, common, Handlebars, html_template) {

    //通用
    function show_common(menuId, para, urls, templates, callback, errorcallback) {
        if (!urls[menuId] || !templates[menuId]) {
            $('#content_body').html(html_template);//加载html模板文本
            //设置多个url和模板
            urls[menuId] = "http://127.0.0.1:8021/" + para;

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
    //保存中间件
    function savemnode(menuId,urls, templates, formdata) {
        var tooltip = $('#vld-tooltip').hide();
        if (formdata) {
            $('#txt_nodename').val(formdata.nodename);
            $('#txt_maccode').val(formdata.machinecode);
            $('#txt_memo').val(formdata.memo);
        } else {
            $('#txt_nodename').val("");
            $('#txt_maccode').val("");
            $('#txt_memo').val("");
            formdata = {
                nodename: $('#txt_nodename').val(),
                machinecode: $('#txt_maccode').val(),
                memo: $('#txt_memo').val()
            };
        }
        $('#node_modal').modal({
            relatedTarget: this,
            closeOnConfirm: false,
            //closeViaDimmer:false,
            //dimmer:false,
            onConfirm: function (e) {
                tooltip.hide();
                if ($('#txt_nodename').val() == "") {
                    tooltip.text("名称不能为空！").show();
                    return;
                }
                if ($('#txt_maccode').val() == "") {
                    tooltip.text("机器码不能为空！").show();
                    return;
                }
                formdata.nodename = $('#txt_nodename').val();
                formdata.machinecode = $('#txt_maccode').val();
                formdata.memo = $('#txt_memo').val();
                common.simpleAjax("http://127.0.0.1:8021/Monitor/SaveMNode", formdata, function (flag) {
                    if (flag) {
                        //$(this).modal('toggle');
                        $(this).modal('close');
                        $('.am-dimmer').remove();
                        showpage(menuId, urls, templates);
                    }
                });
            }
        });
    }

    function regmnode(menuId, urls, templates, formdata) {
        var tooltip = $('#vld-tooltip2').hide();
        $('#txt_maccode2').val(formdata.machinecode);
        $('#txt_regcode').val(formdata.regcode);
        $('#regdate').datepicker('setValue', new Date());
        $('#regcode_modal').modal({
            relatedTarget: this,
            closeOnConfirm: false,
            //closeViaDimmer:false,
            //dimmer:false,
            onConfirm: function (e) {
                tooltip.hide();
                if ($('#txt_regcode').val() == "") {
                    tooltip.text("注册码不能为空！").show();
                    return;
                }
               
                formdata.regcode = $('#txt_regcode').val();
                common.simpleAjax("http://127.0.0.1:8021/Monitor/SaveMNode", formdata, function (flag) {
                    if (flag) {
                        //$(this).modal('toggle');
                        $(this).modal('close');
                        $('.am-dimmer').remove();
                        showpage(menuId, urls, templates);
                    }
                });
            }
        });

        $('#btn_createregcode').unbind('click').click(function () {
            var regdate = $('#regdate').data('date');
            var machinecode = $('#txt_maccode2').val();
            common.simpleAjax("http://127.0.0.1:8021/Monitor/CreateRegCode", { machinecode: machinecode, regdate: regdate }, function (data) {
                if (data) {
                    $('#txt_regcode').val(data.regcode);
                    formdata.regcode = data.regcode;
                    formdata.identify = data.identify;
                }
            });
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
        show_common(menuId, "Monitor/GetMNodeList",urls,templates, function () {
            $('#btn_mnodelist_ref').click(function () {
                showpage(menuId,urls,templates);
            });
            
            $('#btn_mnodelist_add').click(function () {
                savemnode(menuId,urls, templates);
            });

            $('#btn_mnodelist_edit').click(function () {
                var value = $('#content_body table tbody tr.am-active').attr("value");
                value = $.evalJSON(value);
                savemnode(menuId, urls, templates, value);
            });

            $('#btn_mnodelist_reg').click(function () {
                var value = $('#content_body table tbody tr.am-active').attr("value");
                value = $.evalJSON(value);
                regmnode(menuId, urls, templates, value);
            });

            $('#btn_mnodelist_stop').click(function () {
                var value = $('#content_body table tbody tr.am-active').attr("value");
                value = $.evalJSON(value);
                if (value.id_string) {
                    var result = confirm('是否停用此中间件节点？');
                    if (result) {
                        common.simpleAjax("http://127.0.0.1:8021/Monitor/OnOffMidNode", { id: value.id_string }, function (flag) {
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
                    $('#btn_mnodelist_stop').html("<span class='am-icon-twitch'></span> 启用");
                } else {
                    $('#btn_mnodelist_stop').html("<span class='am-icon-remove'></span> 停用");
                }
            });
        });
    }

    return {
        showpage: showpage
    };
});