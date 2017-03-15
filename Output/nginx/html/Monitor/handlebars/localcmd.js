define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/localcmd.html"], function ($, common, Handlebars, html_template) {


    //显示中间件节点
    function show_localcmd(menuId, urls, templates) {
        if (!urls[menuId] || !templates[menuId]) {
            $('#content_body').html(html_template);//加载html模板文本
            //设置多个url和模板
            urls[menuId] ="";
            templates[menuId] = Handlebars.compile($("#" + menuId + "-template").html());
        }
        var context = { data: [] };
        var html = templates[menuId](context);
        $('#content_body').html(html);

        $('#btn_restart').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "quitall", arg: "" }, function (data) {
                    if (data) {
                        common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "startall", arg: "" });
                    }
                });
            }
        });

        $('#btn_restartbase').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartbase", arg: "" });
            }
        });

        $('#btn_restartroute').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartroute", arg: "" });
            }
        });

        $('#btn_restartwebapi').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartwebapi", arg: "" });
            }
        });

        $('#btn_restartmongodb').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartmongodb", arg: "" });
            }
        });

        $('#btn_restartnginx').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "restartnginx", arg: "" });
            }
        });

        $('#btn_exit').click(function () {
            var result = confirm('是否执行此操作？');
            if (result) {
                common.simpleAjax("mnodeconfig/ExecuteCmd", { eprocess: "efwplusserver", method: "exit", arg: "" });
            }
        });
    }

    return {
        showpage: show_localcmd
    };
});