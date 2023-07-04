var connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();

connection.start().then(() => {
    console.log("Huba qosuldu")
})

connection.on("SetOrderStatus", function (status) {
    if (status) {
        toastr["success"]("Order tesdiqlendi!")
    }
    else {
        toastr["error"]("Order tesdiqlenmedi!")
    }
})

$(document).on("click", ".modal-btn", function (e) {
    e.preventDefault();
    let url = $(this).attr("href");
    fetch(url).then(response => {
        if (response.ok) {
            return response.text()
        }
        else {
            alert("Xeta bas verdi")
            return
        }
    })
        .then(data => {
            $("#quickModal .modal-dialog").html(data)
        })
    $("#quickModal").modal("show")
})

$(document).on("click", ".basket-add-btn", function (e) {
    e.preventDefault();
    let url = $(this).attr("href");
    fetch(url).then(response => {
        if (!response.ok) {
            alert("Xeta bas verdi")
        }
        else return response.text()
    }).then(data => {
        $("#basketCart").html(data)
    })
})

toastr.options = {
    "closeButton": false,
    "debug": false,
    "newestOnTop": false,
    "progressBar": false,
    "positionClass": "toast-top-right",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
}


//let btns = document.querySelectorAll(".modal-btn");

//btns.forEach(x => {
//    //x.addEventListener("click", function (e) {
//    //    alert("salam")
//    //})
//    //x.onclick = function (e) {
//    //    alert("salfdfam")
//    //}
//})

