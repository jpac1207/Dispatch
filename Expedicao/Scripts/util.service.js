//This implementation uses closures, that is not supported by internet explorer 11
//Everything is terrible

//window.util = {};

//window.util.service = (function () {
//    return {
//        doAjax(url, data) {
//            var options = {
//                url: url,
//                headers: {
//                    Accept: "application/json"
//                },
//                contentType: "application/json",
//                cache: false,
//                type: 'POST',
//                data: data ? data : null
//            };
//            return $.ajax(options);
//        },
//        stringToDate(inputFormat) {
//            var dateParts = inputFormat.split('/');
//            var date = new Date([dateParts[2], dateParts[1], dateParts[0]].join('/'));
//            return date;
//        }
//    };

//})();

//This is a implementation that uses only user classes and is suported by internet explorer 11

function Util() { };
Util.prototype.doAjax = function (url, data) {
    var options = {
        url: url,
        headers: {
            Accept: "application/json"
        },
        contentType: "application/json",
        cache: false,
        type: 'POST',
        data: data ? data : null
    };
    return $.ajax(options);
}
Util.prototype.stringToDate = function (inputFormat) {
    var dateParts = inputFormat.split('/');
    var date = new Date([dateParts[2], dateParts[1], dateParts[0]].join('/'));
    return date;
}

Util.prototype.modal = function (content) {

    var modal = new tingle.modal({
        footer: true,
        stickyFooter: false,
        closeMethods: ['overlay', 'button', 'escape'],
        closeLabel: "Close",
        cssClass: ['custom-class-1', 'custom-class-2'],
        onOpen: function () {

        },
        onClose: function () {

        },
        beforeClose: function () {            
            return true; // close the modal            
        }
    });

    // set content
    modal.setContent(content);

    // add a button
    modal.addFooterBtn('ok', 'tingle-btn tingle-btn--primary', function () {
        // here goes some logic
        modal.close();
    });

    return modal;
}
