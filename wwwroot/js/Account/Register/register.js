function cleanFormInformation() {
    $(".invalid-feedback").hide();
    $('#Name, #Username, #Email, #Password').removeClass('is-invalid is-valid');
}

function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function validatePassword(password) {
    return password.length >= 6 && /\d/.test(password);
}

function validateField(field, value, validationFunction, errorMessage) {
    if (!validationFunction(value)) {
        field.addClass('is-invalid').removeClass('is-valid');
        field.siblings('.invalid-feedback').text(errorMessage).show();
        return false;
    }
    field.addClass('is-valid').removeClass('is-invalid');
    return true;
}

$(document).ready(function () {
    const formName = '#register-form';

    $(formName).on('submit', function (e) {
        e.preventDefault();
        cleanFormInformation();

        const $name = $('#Name');
        const $username = $('#Username');
        const $email = $('#Email');
        const $password = $('#Password');

        let isValid = true;

        // Validación del nombre
        if ($name.val().length > 15 || $name.val().length === 0) {
            isValid = validateField($name, $name.val(),
                (val) => val.length > 0 && val.length <= 15,
                "El nombre debe tener entre 1 y 15 caracteres");
        }

        // Validación del nombre de usuario
        if ($username.val().length > 15 || $username.val().length === 0) {
            isValid = validateField($username, $username.val(),
                (val) => val.length > 0 && val.length <= 15,
                "El nombre de usuario debe tener entre 1 y 15 caracteres");
        }

        // Validación del email
        isValid = validateField($email, $email.val(), validateEmail,
            "Por favor, introduce un correo electrónico válido") && isValid;

        // Validación de la contraseña
        isValid = validateField($password, $password.val(), validatePassword,
            "La contraseña debe tener al menos 6 caracteres y contener al menos un número") && isValid;

        if (isValid) {
            const formData = {
                Name: $name.val(),
                Username: $username.val(),
                Email: $email.val(),
                Password: $password.val()
            };

            $.ajax({
                url: '/Account/Register',
                type: 'POST',
                data: formData,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            title: 'Registro exitoso',
                            text: '¡Cuenta registrada con éxito! Redirigiendo al panel de control...',
                            icon: 'success',
                            timer: 3000,
                            showConfirmButton: false,
                            didClose: () => {
                                window.location.href = "/Account/Introduction";
                            }
                        });
                    } else {
                        const fieldMap = {
                            'correo electrónico': 'Email',
                            'nombre de usuario': 'Username',
                            'nombre': 'Name',
                            'contraseña': 'Password'
                        };

                        for (const [key, value] of Object.entries(fieldMap)) {
                            if (response.message.toLowerCase().includes(key)) {
                                $(`#${value}`).addClass('is-invalid');
                                $(`#${value}`).siblings('.invalid-feedback').text(response.message).show();
                                break;
                            }
                        }
                    }
                },
                error: function () {
                    Swal.fire({
                        title: 'Error',
                        text: 'Error al intentar registrar la cuenta. Por favor, contacte con soporte técnico.',
                        icon: 'error'
                    });
                }
            });
        }
    });

    // Validación en tiempo real
    $('#Name, #Username').on('input', function () {
        const $field = $(this);
        validateField($field, $field.val(),
            (val) => val.length > 0 && val.length <= 15,
            "El campo debe tener entre 1 y 15 caracteres");
    });

    $('#Email').on('input', function () {
        const $field = $(this);
        validateField($field, $field.val(), validateEmail,
            "Por favor, introduce un correo electrónico válido");
    });

    $('#Password').on('input', function () {
        const $field = $(this);
        validateField($field, $field.val(), validatePassword,
            "La contraseña debe tener al menos 6 caracteres y contener al menos un número");
    });
});