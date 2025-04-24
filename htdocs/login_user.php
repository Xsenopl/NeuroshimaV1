<?php
require_once 'db_connect.php';

function loginUser($mysqli, $email, $password) {
    $stmt = $mysqli->prepare("SELECT nick, haslo FROM uzytkownicy WHERE email = ?");
    $stmt->bind_param("s", $email);

    if (!$stmt->execute()) {
        echo "ERROR_DB";
        return;
    }

    $result = $stmt->get_result();

    if ($result->num_rows === 0) {
        echo "ERROR_NO_USER";
        return;
    }

    $row = $result->fetch_assoc();
    $hashedPassword = $row["haslo"];
    $nick = $row["nick"];

    if (password_verify($password, $hashedPassword)) {
        echo $nick;
    } else {
        echo "ERROR_WRONG_PASSWORD";
    }
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $email = $_POST["email"] ?? "";
    $password = $_POST["password"] ?? "";

    $mysqli = dbConnect();
    loginUser($mysqli, $email, $password);
    $mysqli->close();
}
?>