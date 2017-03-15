define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/register.html"], function ($, common, Handlebars, html_template) {

    //通用
    function show_common(menuId, para, urls, templates, callback, errorcallback) {
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

    //显示中间件节点
    function show_page(menuId, urls, templates) {
        $('#errorinfo').hide();
        show_common(menuId, "mnodeconfig/GetMachineCode",urls,templates, function (data) {
            $('#txt_machinecode').val(data);
            $('#btn_activate').click(function () {
                common.simpleAjax("mnodeconfig/ActivateRegCode", { regcode: $('#txt_regcode').val() }, function (flag) {
                    if (flag) {
                        //alert("激活成功！");
                        $('#errorinfo').text('激活成功！');
                    } else {
                        //alert("激活失败！");
                        $('#errorinfo').text('激活失败！');
                    }

                    $('#errorinfo').show();
                    $('#errorinfo').addClass("am-alert");
                    $('#errorinfo').addClass("am-alert-danger");
                });
            });
        });
    }

    return {
        showpage: show_page
    };
});