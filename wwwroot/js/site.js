// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Confirmación con SweetAlert para los formularios que tengan el atributo data-confirmar.
// Si el formulario tiene data-tipo="eliminar", el botón se muestra en rojo.
document.addEventListener('submit', function (evento) {
    const formulario = evento.target;
    if (!formulario.dataset.confirmar) {
        return;
    }

    evento.preventDefault();
    const esEliminar = formulario.dataset.tipo === 'eliminar';

    Swal.fire({
        title: '¿Está seguro?',
        text: formulario.dataset.confirmar,
        icon: esEliminar ? 'warning' : 'question',
        showCancelButton: true,
        confirmButtonText: esEliminar ? 'Sí, eliminar' : 'Sí, guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: esEliminar ? '#dc3545' : '#0d6efd'
    }).then(function (resultado) {
        if (resultado.isConfirmed) {
            formulario.submit();
        }
    });
});
