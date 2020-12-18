function refreshCities()()
{
    src = $("#Country");
    tgt = $("#Model");

    tgt.empty();

    $.ajax({
        type: 'GET',
        url: 'Home/GetModels',
        dataType: 'json',
        data: { brandName: src.val() },

    })
}