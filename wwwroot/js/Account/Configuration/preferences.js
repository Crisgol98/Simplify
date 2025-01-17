$(document).ready(function () {
    const periods = ["Mañana", "Tarde", "Noche"];

    $(document).on("click", "#addWorkingHour", function () {
        const index = $(".working-hour-entry").length;
        if (index >= periods.length) {
            Swal.fire("Límite alcanzado", "No puedes añadir más períodos.", "warning");
            return;
        }
        $("#workingHoursSection").append(`
            <div class="input-group mb-3 working-hour-entry">
                <select class="form-select period-select" name="Preferences.WorkingHours[${index}].Period" required>
                    <option value="" disabled selected>Selecciona un período</option>
                    ${periods.map(p => `<option value="${p}">${p}</option>`).join("")}
                </select>
                <input type="number" class="form-control" value="1" name="Preferences.WorkingHours[${index}].Hours" min="1" required>
                <button type="button" class="btn btn-danger remove-entry">Eliminar</button>
            </div>
        `);
        updateSelectOptions();
    });
    $(document).on("click", ".remove-entry", function () {
        $(this).closest(".working-hour-entry").remove();
        updateSelectOptions();
    });

    $(document).on("change", ".period-select", updateSelectOptions);

    $(document).on("submit", "#preferencesForm", function (e) {
        e.preventDefault();
        let validPeriods = true;
        $('.period-select').each(function () {
            const value = $(this).val();
            if (!periods.includes(value) || value === "") {
                validPeriods = false;
                return false;
            }
        });
        if (!validPeriods) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Por favor, selecciona un período válido.',
            });
            return;
        }
        const formData = {
            StartTime: $("#StartTime").val(),
            EndTime: $("#EndTime").val(),
            BreakLength: $("#BreakLength").val(),
            BreakFrequency: $("#BreakFrequency").val(),
            WorkingHours: $(".working-hour-entry").map(function () {
                return {
                    Period: $(this).find(".period-select").val(),
                    Hours: $(this).find("input").val()
                };
            }).get()
        };
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
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.href = '/Task/Dashboard';
                        }
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

    updateSelectOptions();
});
