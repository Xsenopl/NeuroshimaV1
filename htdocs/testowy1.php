<?php
require_once 'db_connect.php';

echo "Dziś mamy ". date("Y-m-d H:i:s");

function funtest1($a) {
	$b = ($a + $a);
	return $b;	
}

// Nieużywana
function dodajBitwe($mysqli, $id_pojedynku, $nr_bitwy) {
    $stmt = $mysqli->prepare("INSERT INTO bitwy (id_pojedynku, nr_bitwy) VALUES (?, ?)");
    $stmt->bind_param('ii', $id_pojedynku, $nr_bitwy);
    $stmt->execute();

    if ($stmt->affected_rows <= 0) {
        die("2");	//2 = query failed
    }
}


?>