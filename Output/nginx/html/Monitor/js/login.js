define(['common', 'jquery', 'jquery.cookie'], function (common, $) {
    function login() {
        $('#errorinfo').hide();
        $("#login").submit(function (e) {
            e.preventDefault();
            common.postAjax('http://127.0.0.1:8021/login/submit', $(this).serialize(), function (data) {
                var retobj = data;
                if (retobj.flag) {
                    $.cookie("token", retobj.token);
                    window.location.href = 'index.html';
                } else {
                    //alert("登录失败，用户名或密码错误！");
                    $('#errorinfo').text('登录失败，用户名或密码错误！');
                    $('#errorinfo').show();
                    $('#errorinfo').addClass("am-alert");
                    $('#errorinfo').addClass("am-alert-danger");
                }
            }, function () {
                //alert("登录失败，无法访问服务器！");
                $('#errorinfo').text('登录失败，无法访问服务器！');
                $('#errorinfo').show();
                $('#errorinfo').addClass("am-alert");
                $('#errorinfo').addClass("am-alert-danger");
            });
            return;
        });
    }
    return {
        login: login
    };
});