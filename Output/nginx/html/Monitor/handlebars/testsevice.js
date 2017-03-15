define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/testsevice.html", "amazeui.tree.min"], function ($, common, Handlebars, html_template) {

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

    //
    function show_page(menuId, urls, templates) {
        show_common(menuId, "mnodeconfig/GetAllServices",urls,templates, function (data) {
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
                    common.simpleAjax("mnodeconfig/TestServices", para, function (data) {
                        $('#txt_responsejson').val(data);
                    });
                }
            });
        });
    }

    return {
        showpage: show_page
    };
});