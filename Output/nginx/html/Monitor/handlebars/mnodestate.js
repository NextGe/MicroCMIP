define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/mnodestate.html", "amazeui.tree.min"], function ($, common, Handlebars, html_template) {

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
                callback(common.toJson(data));
            }
        }, errorcallback);
    }

    //
    function showpage(menuId, urls, templates) {
        show_common(menuId, "Monitor/GetMonitorMap", urls, templates, function (data) {
            //data = { childs: data };
            $('#firstTree').tree({
                dataSource: function (options, callback) {
                    // 模拟异步加载
                    setTimeout(function () {
                        callback({ data: options.childs || data });
                    }, 40);
                },
                multiSelect: false,
                cacheItems: true,
                folderSelect: true
            }).on('selected.tree.amui', function (e, selected) {
                if (selected.target.attr && selected.target.attr.identify) {
                    $('body').data('identify', selected.target.attr.identify);
                } else {
                    $('body').data('identify', null);
                }
            });
            //$('#firstTree').tree('discloseAll');
            //刷新
            $('#btn_refresh').click(function () {
                showpage(menuId, urls, templates);
            });
            //查看配置
            $('#btn_config').click(function () {
                var identify = $('body').data('identify');

            });
        });
    }

    return {
        showpage: showpage
    };
});