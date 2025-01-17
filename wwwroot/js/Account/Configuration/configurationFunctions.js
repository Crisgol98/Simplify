function updateSelectOptions() {
    const selectedPeriods = $(".period-select").map((_, el) => $(el).val()).get();
    $(".period-select").each(function () {
        const currentValue = $(this).val();
        $(this).find("option").each(function () {
            const value = $(this).val();
            $(this).prop("disabled", value && selectedPeriods.includes(value) && value !== currentValue);
        });
    });
}