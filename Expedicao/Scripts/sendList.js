//var baseUri = document.getElementById('baseUri').getAttribute("data-save-action-url");
//baseUri = baseUri ? baseUri + "/" : '../Envios/';

//function sendListToExport(sends) {
//    var util = new Util();

//    util.doAjax(baseUri + 'Export', JSON.stringify({ 'sends': sends })).then(function (data) {

//        if (data) {
//            window.location.href = (data);
//        }

//    }, function (motive) {
//        console.log(motive);
//    });
//}

//function run(){

//    if (itens) {
//        sendListToExport(itens);
//    }
//}
//document.getElementById('btnExport').onclick = run;