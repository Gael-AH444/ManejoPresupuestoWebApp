function inicializarFormularioTransacciones(urlObtenerCategorias) {
    $("#tipoOperacionID").change(async function () {
        const valorSeleccionado = $(this).val();

        const respuesta = await fetch(urlObtenerCategorias, {
            method: 'POST',
            body: valorSeleccionado,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const json = await respuesta.json();
        // Se crea un arreglo de opciones
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);

        //Inserta el arreglo de opciones en la lista de CategoriaID
        $("#CategoriaID").html(opciones);

    })
}