$(document).ready(function () {

    $(document).on("click", "#filterAll", function () {
        loadTasks("all", "");
        $('#stateDropdown').text("Todos los estados");
        $('#stateDropdown').append(' <span class="caret"></span>');
    });

    $(document).on("click", "#filterInProcess", function () {
        loadTasks("in_process", "");
        $('#stateDropdown').text("En proceso");
        $('#stateDropdown').append(' <span class="caret"></span>');
    });

    $(document).on("click", "#filterCancelled", function () {
        loadTasks("cancelled", "");
        $('#stateDropdown').text("Cancelado");
        $('#stateDropdown').append(' <span class="caret"></span>');
    });

    $(document).on("click", "#filterCompleted", function () {
        loadTasks("completed", "");
        $('#stateDropdown').text("Finalizado");
        $('#stateDropdown').append(' <span class="caret"></span>');
    });


    $(document).on("click", "#filterPriorityDefault", function () {
        loadTasks("all", "");
        $('#priorityDropdown').text("Seleccionar prioridad");
        $('#priorityDropdown').append(' <span class="caret"></span>');
    });

    $(document).on("click", "#filterHigh", function () {
        loadTasks("all", "Alta");
        $('#priorityDropdown').text("Alta");
        $('#priorityDropdown').append(' <span class="caret"></span>');
    });

    $(document).on("click", "#filterMedium", function () {
        loadTasks("all", "Media");
        $('#priorityDropdown').text("Media");
        $('#priorityDropdown').append(' <span class="caret"></span>');
    });

    $(document).on("click", "#filterLow", function () {
        loadTasks("all", "Baja");
        $('#priorityDropdown').text("Baja");
        $('#priorityDropdown').append(' <span class="caret"></span>');
    });


    $(document).on('click', '#searchButton', function () {
        searchTasks();
    });


    $(document).on('keypress', '#searchInput', function (e) {
        if (e.which === 13) {
            searchTasks();
        }
    });


    loadTasks("all", "");
});




function searchTasks() {
    const searchText = $('#searchInput').val().trim();
    const category = $('#stateDropdown').val();
    const priority = $('#priorityDropdown').val();

    $.ajax({
        url: '/Task/Search',
        method: 'GET',
        data: {
            searchText: searchText,
            category: category,
            priority: priority
        },
        success: function (response) {
            $("#task-list-container").empty();

            if (response.length > 0) {
                hideNoTasksMessage();

                response.forEach(task => {
                    const taskCard = generateTaskCard(task);
                    $("#task-list-container").append(taskCard);
                });
            } else {

                showNoTasksMessage();
            }
        },
        error: function () {
            alert('Error al realizar la búsqueda.');
        }
    });
}

function showNoTasksMessage() {
    $(".no-tasks-message").removeClass("d-none");
}

function hideNoTasksMessage() {
    $(".no-tasks-message").addClass("d-none");
}

function loadTasks(category, priority) {
    $.ajax({
        url: '/Task/GetFilteredTasks',
        method: 'GET',
        data: {
            category: category,
            priority: priority
        },
        success: function (data) {

            $(".task-list").empty();

            if (data.length === 0) {
                showNoTasksMessage();
            } else {
                hideNoTasksMessage();

                data.forEach(function (task) {
                    const taskHtml = generateTaskCard(task);
                    $(".task-list").append(taskHtml);
                });
            }
        },
        error: function () {
            alert("Error al cargar las tareas");
        }
    });
}
function generateTaskCard(task) {
    return `
                            <div onclick="showTaskDetails(${task.id})" class="col-12 col-md-6 col-lg-4 mb-4">
                                <div class="task-card card shadow-sm" style="border-radius: 8px;">
                                    <div class="card-body">
                                        <h5 class="card-title">${task.name}</h5>
                                        <p class="card-text">${task.description}</p>
                                        <span class="badge bg-primary">${task.priority}</span>
                                        <span class="badge bg-secondary">${task.state}</span>
                                    </div>
                                </div>
                            </div>
                        `;
}