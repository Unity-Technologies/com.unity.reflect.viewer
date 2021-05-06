<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "tpdemo";
//$tableName = "tptiles";
$csvPath = $_POST["csvPath"];
$tableName = $_POST["tableName"];
//$csvPath = "C:/Users/aca/Documents/Projects/BIMEXPO/DB_Carrelages_Demo.csv";

echo "Received csvPath: " . $csvPath;
echo "Received tableName: " . $tableName;

try
{
	$bdd = new PDO('mysql:host=localhost;dbname=tpdemo;charset=utf8', 'root', '', array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::MYSQL_ATTR_LOCAL_INFILE => true));
}
catch(Exception $e)
{
	die('Error : ' . $e->getMessage());
}

//Remove DB if exists already
$drop = $bdd->query('DROP TABLE IF EXISTS ' . $tableName);
if($drop->errorCode() == 00000)
{
	echo "The table " . $tableName . " was correctly deleted from DB!";
}
else
{
	echo "Error: The table " . $tableName . " could not be deleted from DB!";
}

//Close the query access
$drop->closeCursor();

//Re-create table
$create = "CREATE TABLE IF NOT EXISTS " . $tableName . " ( code_lot VARCHAR(20), code_art VARCHAR(20), libelle VARCHAR(100), prix_vente VARCHAR(10), date_maj_pv VARCHAR(10), reimpression VARCHAR(10),";
$create = $create . " prix_anticipe VARCHAR(10), localisation VARCHAR(15), hs VARCHAR(10), etiquette_affiche VARCHAR(20), unite VARCHAR(5), type_art VARCHAR(10), conso VARCHAR(10),";
$create = $create . " ref VARCHAR(5), categorie VARCHAR(10), type_mat VARCHAR(10), resist_abrasion VARCHAR(10), resist_rayure VARCHAR(10), rectifie VARCHAR(10), etat_surface VARCHAR(10),";
$create = $create . " couleur VARCHAR(10), variation_chrom VARCHAR(5), couche_usure VARCHAR(10), chemin_texture VARCHAR(100), mur TINYINT NOT NULL, sol TINYINT NOT NULL, sdb TINYINT NOT NULL, id SMALLINT AUTO_INCREMENT PRIMARY KEY)";
$create = $create . "CHARACTER SET 'utf8' ENGINE=INNODB;";

$result = $bdd->query($create);
if ($result->errorCode() == 00000) 
{
  echo "Table created!";
} 
else 
{
  echo "0 lines inserted";
}

//Close the query access
$result->closeCursor();

//Fill the table
//!!Path with FORWARD SLASHES!!
$fillCommand = "LOAD DATA LOCAL INFILE '" . $csvPath . "' INTO TABLE tptiles FIELDS TERMINATED BY ',' LINES TERMINATED BY '\r\n' IGNORE 1 LINES;";

$fillResult = $bdd->query($fillCommand);

if ($fillResult) 
{
	echo "CSV loaded!";
} 
else 
{
  echo "0 lines inserted";
}

//Close the query access
$fillResult->closeCursor();

//Remove useless columns
$removeCommand = "ALTER TABLE " . $tableName;
$uselessCols = array("code_lot", "code_art", "prix_vente", "date_maj_pv", "reimpression", "prix_anticipe", "localisation", "hs", "etiquette_affiche", "unite", "type_art", "conso", "ref", "categorie", "type_mat", "resist_abrasion", "resist_rayure", "rectifie", "etat_surface", "couleur", "variation_chrom", "couche_usure");
$count = 0;

foreach ($uselessCols as $value) 
{
	if ($count == 0) 
	{
		$removeCommand = $removeCommand . " DROP " . $value;
	}
    else 
    {
		$removeCommand = $removeCommand . ", DROP " . $value;
    }
	$count = $count + 1;
}
$removeCommand = $removeCommand . ";";

$removeResult = $bdd->query($removeCommand);

//Close the query access
$removeResult->closeCursor();


?> 