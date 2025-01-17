$(document).ready(function () {
    const periods = ["Mañana", "Tarde", "Noche"];
    $(".step").removeClass("active");
    $("#step-1").addClass("active");

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

    $(".btn-next").on("click", function () {
        const currentStep = $(this).closest(".step");
        const nextStepId = $(this).data("next");

        const timeInput = currentStep.find("input[type='time']");
        if (timeInput.length && !timeInput.val()) {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Por favor, completa este campo antes de continuar.",
                confirmButtonText: "Aceptar"
            });
            return;
        }

        currentStep.removeClass("active");
        $(nextStepId).addClass("active");
    });

    $(".btn-prev").on("click", function () {
        const currentStep = $(this).closest(".step");
        const prevStepId = $(this).data("prev");

        currentStep.removeClass("active");
        $(prevStepId).addClass("active");
    });

    $(".btn-submit").on("click", function () {
        const timeInputs = {
            startTime: $("#start-time").val(),
            endTime: $("#end-time").val(),
            breakLength: $("#break-length").val(),
            breakFrequency: $("#break-frequency").val(),
            workingHours: $(".working-hour-entry").map(function () {
                return {
                    period: $(this).find(".period-select").val(),
                    hours: $(this).find("input").val()
                };
            }).get()
        };

        if (!Object.values(timeInputs).every(val => val)) {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Por favor, completa todos los campos.",
                confirmButtonText: "Aceptar"
            });
            return;
        }

        $.ajax({
            url: "/Account/UpdatePreferences",
            type: "POST",
            data: timeInputs,
            success: function () {
                Swal.fire({
                    icon: "success",
                    title: "Éxito",
                    text: "Tus preferencias han sido guardadas.",
                    confirmButtonText: "Aceptar"
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = '/Task/Dashboard';
                    }
                });
            },
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Hubo un problema al guardar las preferencias.",
                    confirmButtonText: "Aceptar"
                });
            }
        });
    });

    updateSelectOptions();
});
