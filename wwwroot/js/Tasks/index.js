$(document).ready(function () {
    $.ajax({
        url: "/Task/GetTasks",
        type: "GET",
        success: function (result) {
            $('#calendar').fullCalendar({
                header: {
                    left: 'prev,next today',
                    center: 'title'
                },
                defaultView: 'month',
                events: [
                    {
                        title: 'Evento de prueba',
                        start: '2024-08-14T10:00:00',
                        end: '2024-12-14T12:00:00'
                    }
                ]
            });
        },
        error: function (err) {

        }
    })
});
