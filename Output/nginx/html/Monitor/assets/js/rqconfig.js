require.config({
    baseUrl: 'assets/js',
    paths: {
        text:'text',
        jquery: 'jquery.min',
        common: '../../js/common',
        index: '../../js/index',
        login:"../../js/login"
    },
    shim: {
        "jquery.cookie": ["jquery"],
        "amazeui.tree.min": ["jquery"],
        "jquery.formatDateTime.min":["jquery"],
        "app": ["jquery"],
        "index": ["jquery", "jquery.cookie", "director.min", "amazeui.tree.min", "jquery.formatDateTime.min"]
    }
});

//requirejs(["amazeui.min","app", "index"]);