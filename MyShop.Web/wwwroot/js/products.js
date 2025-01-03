﻿
var dtble;

$(document).ready(function () {
    loadData();
});

function loadData() {
    dtble = $("#productsTable").DataTable({
        "ajax" : {
            url: "/Admin/Product/GetData",
            //dataSrc : "data"
        },
        "columns": [
            { data: "name" },
            { data: "description" },
            { data: "price" },
            { data: "category.name" },
            {
                data: "id",
                render: function (data) {
                    return `
                        <a href="/Admin/Product/Edit/${data}" class="btn btn-success" >Edit</a>
                        <a onclick=DeleteItem("/Admin/Product/Delete/${data}") class="btn btn-danger" >Delete</a>
                    `;
                } 
            }
        ]
        
    });
}

function DeleteItem(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            // delete the record from database
            $.ajax({
                url: url,
                type: "delete",
                success: function (data) {
                    if (data.success) {
                        // refresh the table data
                        dtble.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
            Swal.fire({
                title: "Deleted!",
                text: "Your file has been deleted.",
                icon: "success"
            });
        }
    });
}