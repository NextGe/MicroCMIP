define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/debuglog.html"], function ($, common, Handlebars, html_template) {

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

    //
    function show_page(menuId, urls, templates) {
        show_common(menuId, "mnodeconfig/debuglog?logtype=MidLog&date=",urls,templates, function () {
            $('#logdate').datepicker('setValue', new Date());
            if ($('body').data('logtype')) {
                $('#logtype').val($('body').data('logtype'));
                //$('#logtype').find('option[value="' + $('body').data('logtype') + '"]').attr('selected', true);
            }
            $('#logtype').selected();

            $('#btn_logsearch').click(function () {
                $('body').data('logtype', $('#logtype').val());
                urls[menuId] = "mnodeconfig/debuglog?logtype=" + $('#logtype').val() + "&date=" + $('#logdate').data('date');
                show_page(menuId,urls,templates);
            });
        });
    }

    return {
        showpage: show_page
    };
});