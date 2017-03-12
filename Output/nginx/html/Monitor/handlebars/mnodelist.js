define(['jquery', 'common', "handlebars.min", "text!../../Monitor/handlebars/mnodelist.html"], function ($, common, Handlebars, html_template) {

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

    //显示中间件节点
    function show_mnodelist(menuId, urls, templates) {
        Handlebars.registerHelper("todelflag", function (value) {
            if (value == "0") {
                return "正常";
            } else {
                return "停用";
            }
        });
        show_common(menuId, "Monitor/GetMNodeList",urls,templates, function () {
            $('#btn_mnodelist_ref').click(function () {
                show_mnodelist(menuId);
            });

            $('#btn_mnodelist_add').click(function () {
                $('#node_modal').modal({
                    relatedTarget: this,
                    onConfirm: function (e) {
                        alert('你输入的是：' + e.data || '')
                    },
                    onCancel: function (e) {
                        //alert('不想说!');
                    }
                });
            });

            $('#btn_mnodelist_edit').click(function () {
                $('#node_modal').modal('open');
            });

            $('#btn_mnodelist_reg').click(function () {
                $('#regdate').datepicker('setValue', new Date());
                $('#regcode_modal').modal('open');
            });

            $('#btn_mnodelist_stop').click(function () {
                var id = $('#content_body table tbody tr.am-active').attr("value");
                if (id) {
                    var result = confirm('是否停用此中间件节点？');
                    if (result) {
                        common.simpleAjax("http://127.0.0.1:8021/Monitor/OnOffMidNode", { id: id }, function (flag) {
                            if (flag) {
                                show_mnodelist(menuId);
                            }
                        });
                    }
                }
            });

            $('#content_body table tbody tr').click(function () {
                $('#content_body table tbody tr').removeClass("am-active");
                $(this).addClass("am-active");
                var delflag = $(this).attr("delflag");
                if (delflag == "1") {
                    $('#btn_mnodelist_stop').html("<span class='am-icon-twitch'></span> 启用");
                } else {
                    $('#btn_mnodelist_stop').html("<span class='am-icon-remove'></span> 停用");
                }
            });
        });
    }

    return {
        showpage: show_mnodelist
    };
});