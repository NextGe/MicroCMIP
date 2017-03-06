require.config({
    baseUrl: 'assets/js',
    paths: {
        jquery: 'jquery.min',
        common: '../../js/common',
        index: '../../js/index',
        login:"../../js/login"
    },
    shim: {
        "jquery.cookie": ["jquery"],
        "app": ["jquery"],
        "index": ["jquery", "jquery.cookie", "director.min"]
    }
});

//requirejs(["amazeui.min","app", "index"]);