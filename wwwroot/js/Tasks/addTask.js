$(document).ready(function () {
    $(document).on("click", "#btn-add-task", function () {
        // Obtener el valor del campo time (HH:mm)
        const estimatedTime = $('#estimatedTime').val();

        // Convertir el valor de time (HH:mm) a minutos
        const timeParts = estimatedTime.split(':'); // Dividir en horas y minutos
        const hours = parseInt(timeParts[0]); // Horas
        const minutes = parseInt(timeParts[1]); // Minutos

        // Calcular el tiempo total en minutos
        const totalEstimatedMinutes = (hours * 60) + minutes;

        // Crear el objeto task con el tiempo estimado en minutos
        const task = {
            UserId: 1,
            Name: $('#name').val(),
            Description: $('#description').val(),
            Priority: $('#priority').val(),
            DueDate: $('#dueDate').val(),
            EstimatedTime: totalEstimatedMinutes // Pasar el tiempo estimado en minutos
        };

        // Enviar los datos al servidor mediante AJAX
        $.ajax({
            url: '/Task/AddTask',
            type: 'POST',
            data: task,
            success: function (response) {
                alert('Task added successfully!');
                $('#add-task-form')[0].reset(); // Limpiar el formulario
            },
            error: function (xhr, status, error) {
                alert('Failed to reach the controller to add the task');
            }
        });
    });
});