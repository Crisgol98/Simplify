$(document).ready(function () {

    $(".list-group-item").click(function (e) {
        e.preventDefault();


        $(".list-group-item").removeClass("active");
        $(this).addClass("active");


        var url = $(this).attr("href");
        $("#main-content").load(url);
    });
    $(document).on("click", "#btn-close-modal", function () {
        location.reload();
    });
});