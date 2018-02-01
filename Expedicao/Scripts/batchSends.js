var baseUri = document.getElementById('baseUri').getAttribute("data-save-action-url");
baseUri = baseUri ? baseUri + "/" : '../Envios/';

function showMessage(message, cssClass) {
    //var label = document.getElementById('message');
    //label.classList.add(cssClass);
    //label.innerHTML = message;

    var util = new Util();
    var modal = util.modal('<div class=" alert ' + cssClass + '">' + message + '</div>');
    modal.open();
}

function disableButton(btn) {
    btn.style.pointerEvents = 'none';
    btn.style.cursor = 'not-allowed';
    btn.innerText = "Aguarde!";
}

function enableButton(btn) {
    btn.style.pointerEvents = 'auto';
    btn.style.cursor = 'auto';
    btn.innerText = "Enviar Todos";
}

function getDataFromTable() {
    var sends = new Array();
    var sedeOrigem = document.getElementById('SedeOrigemId').value;
    var sedeDestino = document.getElementById('SedeDestinoId').value;
    var sendDate = document.getElementById('sendDate').value;
    var table = document.getElementById("tbSends");
    var tbody = table.children[0];
    var rows = tbody.children;

    for (var i = 1; i < rows.length; i++) {
        var cells = rows[i].cells;
        var cellsArray = [].slice.call(cells);
        //var controlsOfEachCell = cellsArray.map(x => x.children[0]);
        var controlsOfEachCell = new Array();
        //getting controls inside each cell, because map is not supported in internet explorer
        for (var c = 0; c < cellsArray.length; c++) {
            controlsOfEachCell.push(cellsArray[c].children[0]);
        }

        var description = controlsOfEachCell[0].value;
        var qtd = controlsOfEachCell[1].value;
        var ordemManut = controlsOfEachCell[2].value;
        var numeroSerie = controlsOfEachCell[3].value;
        var tipoTransporte = controlsOfEachCell[4].value;
        var notaFiscal = controlsOfEachCell[5].value;
        var motivo = controlsOfEachCell[6].value;
        var numeroImpressaoNota = controlsOfEachCell[7].value;
        var notaSap = controlsOfEachCell[8].value;
        var idMateriais = controlsOfEachCell[9].value;

        if (description) {
            if (qtd) {
                var envio = new EnvioViewModel();
                envio.OrdemManutencao = ordemManut;
                envio.Descricao = description;
                envio.Quantidade = qtd;
                envio.NumeroSerie = numeroSerie;
                envio.TipoTransporteId = tipoTransporte;
                envio.NotaFiscal = notaFiscal;
                envio.MotivoId = motivo;
                envio.NumeroImpressaoNota = numeroImpressaoNota;
                envio.NotaTransferenciaSap = notaSap;
                envio.IdSolicitacao = idMateriais;
                envio.SedeOrigemId = sedeOrigem;
                envio.SedeDestinoId = sedeDestino;
                sends.push(envio);
            }
            else {
                showMessage("Descrição e Quantidade são campos obrigatórios!", "alert-danger");
                return;
            }
        }

    }

    if (sends && sends.length > 0) {

        var util = new Util();
        util.doAjax(baseUri + 'SendInBatch', JSON.stringify({ 'sends': sends, 'sendDate': sendDate })).then(function (data) {

            if (data && data.length > 1) {
                showMessage(data[0], data[1]);

                if (data[0] == "Sucesso ao enviar!")
                    setTimeout(function () { location.reload(true); }, 2000);
            }

        }, function (motive) {
            console.log(motive);
        });

        return true;
    } else {
        showMessage('Nenhum material a ser enviado', "alert-info");
    }
}

function run() {
    var btn = document.getElementById('actionButton');
    disableButton(btn);
    getDataFromTable();
    enableButton(btn);
}

document.getElementById('actionButton').onclick = run;