$(document).ready(function () {

    const periods = ["Mañana", "Tarde", "Noche"];


    function updateSelectOptions() {
        const selectedPeriods = $(".period-select").map(function () {
            return $(this).val();
        }).get();

        $(document).on("each", ".period-select", function () {
            const currentValue = $(this).val();
            $(this).find("option").each(function () {
                const value = $(this).val();
                if (value && selectedPeriods.includes(value) && value !== currentValue) {
                    $(this).prop("disabled", true);
                } else {
                    $(this).prop("disabled", false);
                }
            });
        });
    }


    $(document).on("click", "#addWorkingHour", function () {
        const index = $(".working-hour-entry").length;
        if (index >= periods.length) {
            alert("No puedes añadir más períodos. Todos los períodos ya están seleccionados.");
            return;
        }

        const newEntry = `
            <div class="input-group mb-3 working-hour-entry">
                <select class="form-select period-select"
                        name="Preferences.WorkingHours[${index}].Period" required>
                    <option value="" selected disabled>Selecciona un período</option>
                    ${periods.map(p => `<option value="${p}">${p}</option>`).join("")}
                </select>
                <input type="number" class="form-control"
                       name="Preferences.WorkingHours[${index}].Hours" min="0" required>
                <button type="button" class="btn btn-danger remove-entry">Eliminar</button>
            </div>`;
        $("#workingHoursSection").append(newEntry);
        updateSelectOptions();
    });


    $(document).on("click", ".remove-entry", function () {
        $(this).closest(".working-hour-entry").remove();
        updateSelectOptions();
    });


    $(document).on("change", ".period-select", function () {
        updateSelectOptions();
    });


    updateSelectOptions();




    $(document).on("submit", '#preferencesForm', function (event) {
        event.preventDefault();

        var formData = {
            'StartTime': $('#StartTime').val(),
            'EndTime': $('#EndTime').val(),
            'BreakLength': $('#BreakLength').val(),
            'BreakFrequency': $('#BreakFrequency').val(),
            'WorkingHours': []
        };

        $('#workingHoursSection .input-group').each(function () {
            var period = $(this).find('select.period-select').val();
            var hours = parseInt($(this).find('input[name*="Hours"]').val(), 10);

            if (period && !isNaN(hours)) {
                formData.WorkingHours.push({
                    'Period': period,
                    'Hours': hours
                });
            }
        });

        console.log(JSON.stringify(formData, null, 2)); // Depuración

        $.ajax({
            url: '/Account/UpdatePreferences',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Preferencias actualizadas',
                        text: 'Tus preferencias han sido guardadas correctamente.',
                        confirmButtonText: 'Aceptar'
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message,
                        confirmButtonText: 'Aceptar'
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Error en la solicitud',
                    text: 'Hubo un problema al procesar tu solicitud. Intenta nuevamente más tarde.',
                    confirmButtonText: 'Aceptar'
                });
            }
        });
    });

});