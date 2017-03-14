define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/mnodestate.html", "amazeui.tree.min"], function ($, common, Handlebars, html_template) {

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
                callback(common.toJson(data));
            }
        }, errorcallback);
    }

    //
    function showpage(menuId, urls, templates) {
        show_common(menuId, "Monitor/GetMonitorMap", urls, templates, function (data) {
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
            })
        });
    }

    return {
        showpage: showpage
    };
});