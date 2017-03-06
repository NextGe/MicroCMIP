define(['common','jquery','jquery.cookie'], function (common,$) {
    $("#login").submit(function (e) {
        e.preventDefault();
        common.postAjax('http://127.0.0.1:8021/login/submit', $(this).serialize(), function (data) {
            var retobj = data;
            if (retobj.flag) {
                $.cookie("token", retobj.token);
                window.location.href = 'index.html';
            } else {
                alert("登录失败，用户名或密码错误！");
            }
        }, function () {
            alert("登录失败，无法访问服务器！");
        });
        return;
    });
});