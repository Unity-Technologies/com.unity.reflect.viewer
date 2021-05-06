using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
//using System.Windows.Forms;
//using MySql.Data.MySqlClient;
using UnityEngine.UIElements;

public class DBInteractions : MonoBehaviour
{
    [Header("DATABASE")]
    public string host;
    public string database, username, password, tilesTable;
    [Header("PROJECT DETAILS")]
    public string clientId;
    public string projectId;

    //private MySqlConnection con;

    /*
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Connect_DB();
        }
        catch 
        {
            return;
        }
        try
        {
            //createMySQLTableFromCSV(@"C:\Users\aca\Documents\Projects\BIMEXPO" + "\\DB_Carrelages_Demo.csv", database);
            createBuildingTable();

            //Create the user's choices DB
            string createCmd = "CREATE TABLE IF NOT EXISTS c" + clientId + "_p" + projectId + "_choices ( id_surface SMALLINT UNSIGNED NOT NULL, id_tile SMALLINT UNSIGNED NOT NULL, PRIMARY KEY (id_surface) ) CHARACTER SET 'utf8' ENGINE=INNODB;";
        
            MySqlCommand cmdSql = new MySqlCommand(createCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        { 
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Gets the list of the names ('libelles') of all the preselected tiles in the project.
    /// </summary>
    /// <returns>The list of tiles.</string></returns>
    public List<string> RetrievePreselectedTiles()
    {
        Connect_DB();
        MySqlCommand joinTables = new MySqlCommand("SELECT tptiles.libelle FROM tptiles INNER JOIN preselections ON tptiles.id = preselections.tile_id;", con);
        MySqlDataReader myReader = joinTables.ExecuteReader();

        List<string> selectedTiles = new List<string>();
        while (myReader.Read())
        {
            selectedTiles.Add(myReader["libelle"].ToString());
        }
        return selectedTiles;
    }

    /// <summary>
    /// Given a tile name ('libelle'), finds the path to its texture, which is located in the table 'chemin_texture' column.
    /// For the moment this path is simply the name of the folder in which the textures are stored for a given tile.
    /// </summary>
    /// <param name="name">The name of the tile (i.e. the 'libelle').</param>
    /// <returns>The path to the texture, as stored in the table.</returns>
    public string GetTexturePathFromName(string name)
    {
        string data = null;
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT * FROM `" + tilesTable + "` WHERE `libelle`='" + name + "'", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data = myReader["chemin_texture"].ToString();
            }
            if (data == null)
                Debug.Log("Image not found!");
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return data;
    }

    /// <summary>
    /// Given a tile name ('libelle'), finds the corresponding tile id, which is located in the table 'id' column.
    /// </summary>
    /// <param name="name">The name of the tile (i.e. the 'libelle').</param>
    /// <returns>The id of the tile, as stored in the table.</returns>
    public string GetTileIdFromName(string name)
    {
        string data = null;
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT id FROM `" + tilesTable + "` WHERE `libelle`='" + name + "'", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data = myReader["id"].ToString();
            }
            if (data == null)
                Debug.Log("Id not found!");
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return data;
    }

    /// <summary>
    /// Establish a connection to the database with the credentials provided in the public variables.
    /// </summary>
    public void Connect_DB()
    {
        string cmd = "SERVER=" + host + ";port=3306;database=" + database + ";USER ID=" + username + ";PASSWORD=" + password + ";Pooling=true";
        try
        {
            con = new MySqlConnection(cmd);
            con.Open();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MessageBox.Show("Erreur de connection avec la base de données. Merci de vérifier que celle-ci est accessible.");
            throw new Exception("DB connection error");
        }
    }

    public void changeMaterial(GameObject obj, string texturePath)
    {
        //Create a new Material with standard shader
        Material newMat = new Material(Shader.Find("Standard"));

        //Fetch the texture from disk
        Texture newTexture = (Texture)LoadTextureFromDisk(texturePath);

        //Assign texture to material
        newMat.mainTexture = newTexture;

        //Assign the newly created Material onto the object
        obj.GetComponent<Renderer>().material = newMat;

        //StartCoroutine(GetTextureFromPC(url,obj));
    }

    public Texture2D LoadTextureFromDisk(string FilePath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails
        Texture2D Tex2D;
        byte[] FileData;
        string[] filesInDir;

        //Files in the directory
        filesInDir = Directory.GetFiles(FilePath);

        //Get the 1st image within directory
        string picture = filesInDir[0];

        if (File.Exists(picture))
        {
            Debug.Log("File exists!");
            FileData = File.ReadAllBytes(picture);
            Tex2D = new Texture2D(2, 2);                // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))              // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                           // If data = readable -> return texture
        }
        Debug.Log("File doesn't exist!");
        return null;                                    // Return null if load failed
    }

    public void createMySQLTableFromCSV(string csvFilePath, string DBName) 
    {
        string deleteTableCmd = "DROP TABLE IF EXISTS " + tilesTable + ";";

        string createCmd = "CREATE TABLE IF NOT EXISTS " + tilesTable + " ( code_lot VARCHAR(20), code_art VARCHAR(20), libelle VARCHAR(100), prix_vente VARCHAR(10), date_maj_pv VARCHAR(10), reimpression VARCHAR(10),";
        createCmd = createCmd + " prix_anticipe VARCHAR(10), localisation VARCHAR(15), hs VARCHAR(10), etiquette_affiche VARCHAR(20), unite VARCHAR(5), type_art VARCHAR(10), conso VARCHAR(10),";
        createCmd = createCmd + " ref VARCHAR(5), categorie VARCHAR(10), type_mat VARCHAR(10), resist_abrasion VARCHAR(10), resist_rayure VARCHAR(10), rectifie VARCHAR(10), etat_surface VARCHAR(10),";
        createCmd = createCmd + " couleur VARCHAR(10), variation_chrom VARCHAR(5), couche_usure VARCHAR(10), chemin_texture VARCHAR(100), mur TINYINT NOT NULL, sol TINYINT NOT NULL, sdb TINYINT NOT NULL, id SMALLINT AUTO_INCREMENT PRIMARY KEY)";
        createCmd = createCmd + "CHARACTER SET 'utf8' ENGINE=INNODB;";

        //!!Path with FORWARD SLASHES!!
        string fillCommand = "LOAD DATA LOCAL INFILE 'C:/Users/aca/Documents/Projects/BIMEXPO/DB_Carrelages_Demo.csv' INTO TABLE tptiles FIELDS TERMINATED BY ',' LINES TERMINATED BY '\r\n' IGNORE 1 LINES;"; // (@col1,@col2,@col3,@col4,@col5,@col6,@col7,@col8,@col9,@col10,@col11,@col12,@col13,@col14,@col15,@col16,@col17,@col18,@col19,@col20,@col21,@col22,@col23,@col24,@col25,@col26,@col27) SET 'libelle'=@col3,'chemin_texture'=@col24,'mur'=@col25,'sol'=@col26,'sdb'=@col27;";

        //Remove useless columns
        string removeCommand = "ALTER TABLE " + tilesTable;
        List<string> uselessCols = new List<string>() { "code_lot", "code_art", "prix_vente", "date_maj_pv", "reimpression", "prix_anticipe", "localisation", "hs", "etiquette_affiche", "unite", "type_art", "conso", "ref", "categorie", "type_mat", "resist_abrasion", "resist_rayure", "rectifie", "etat_surface", "couleur", "variation_chrom", "couche_usure" };
        int count = 0;
        foreach (string item in uselessCols)
        {
            if (count == 0)
                removeCommand = removeCommand + " DROP " + item;
            else
                removeCommand = removeCommand + ", DROP " + item;
            count += 1;
        }
        removeCommand = removeCommand + ";";

        try
        {
            Connect_DB();

            MySqlCommand cmdSql0 = new MySqlCommand(deleteTableCmd, con);
            MySqlDataReader myReader0 = cmdSql0.ExecuteReader();
            myReader0.Close();

            MySqlCommand cmdSql = new MySqlCommand(createCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();

            MySqlCommand cmdSql2 = new MySqlCommand(fillCommand, con);
            MySqlDataReader myReader2 = cmdSql2.ExecuteReader();
            myReader2.Close();

            MySqlCommand cmdSql3 = new MySqlCommand(removeCommand, con);
            MySqlDataReader myReader3 = cmdSql3.ExecuteReader();
            myReader3.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return;
        }
        GameObject.FindGameObjectWithTag("Player").GetComponent<MenusHandler>().ActivatePreselectionMenu();
    }

    /// <summary>
    /// Function <c>ListAllTileNamesInDB</c> lists all the tiles names ('libelle') present in the tiles table in the DB.
    /// </summary>
    /// <returns>A List of strings of all the 'libelle'.</returns>
    public List<string> ListAllTileNamesInDB()
    {
        string data = null;
        List<string> libelles = new List<string>();
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT `libelle` FROM `" + tilesTable + "` WHERE `libelle` IS NOT NULL", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data = myReader["libelle"].ToString();
                if (data == null)
                    Debug.Log("Entry not found!");
                else
                {
                    libelles.Add(data);
                }
            }
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return libelles;
    }

    //public List<string> FilterWallsOnlyFromDB(string DBName)

    /// <summary>
    /// Given a list of tiles names (i.e. 'libelles'), retrieves only the ones that are suitable for walls, as a new list of libelles.
    /// </summary>
    /// <param name="list">A List of string that are the libelles to be filtered.</param>
    /// <returns>The filtered List of libelles.</returns>
    public List<string> FilterWallsOnlyFromTileList(List<string> list)
    {
        List<string> data = new List<string>();
        List<string> filteredList = new List<string>();
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT `libelle` FROM `" + tilesTable + "` WHERE `mur`=1", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data.Add(myReader["libelle"].ToString());
            }
            if (data == null)
                Debug.Log("No compatible tiles found!");
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex.Message);
        }
        foreach (string item in list)
        {
            if (data.Contains(item))
                filteredList.Add(item);
        }
        return filteredList;
    }

    public List<string> FilterSlabsOnlyFromTileList(List<string> list)
    {
        List<string> data = new List<string>();
        List<string> filteredList = new List<string>();
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT `libelle` FROM `" + tilesTable + "` WHERE `sol`=1", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data.Add(myReader["libelle"].ToString());
            }
            if (data == null)
                Debug.Log("No compatible tiles found!");
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        foreach (string item in list)
        {
            if (data.Contains(item))
                filteredList.Add(item);
        }
        return filteredList;
    }

    public void SaveUserChoiceToDB(string tileId, string surfaceId)
    {
        //Create the user's choices DB - Using REPLACE to authorize the overwriting (if client changes its mind)
        string insertCmd = "REPLACE INTO c" + clientId + "_p" + projectId + "_choices VALUES ( " + surfaceId + ", " + tileId + ")";
        try
        {
            MySqlCommand cmdSql = new MySqlCommand(insertCmd, con);
            cmdSql.ExecuteNonQuery();
            con.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    void createBuildingTable()
    {
        //Create the table
        string deleteTable = "DROP TABLE IF EXISTS c" + clientId + "_p" + projectId + "_surfaces;";
        string createCmd = "CREATE TABLE IF NOT EXISTS c" + clientId + "_p" + projectId + "_surfaces ( id_surface SMALLINT UNSIGNED NOT NULL, room_name VARCHAR(20), level TINYINT, surface_group SMALLINT, PRIMARY KEY (id_surface) ) CHARACTER SET 'utf8' ENGINE=INNODB;";
        try
        {
            MySqlCommand cmdSql = new MySqlCommand(deleteTable + createCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

        //Populate the table
        string insertCmd = "INSERT INTO c" + clientId + "_p" + projectId + "_surfaces (id_surface, room_name, level, surface_group) VALUES";
        int count = 0;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.layer == 6 || go.layer == 7)
            {
                if (count > 0)
                {
                    insertCmd = insertCmd + ", ";
                }
                insertCmd = insertCmd + "( '" + go.GetComponent<dummyMetadataScript>().ID + "', '" + go.GetComponent<dummyMetadataScript>().room + "', '" + go.GetComponent<dummyMetadataScript>().level + "', NULL)";
                count += 1;
            }
        }
        insertCmd = insertCmd + ";";

        try
        {
            MySqlCommand cmdSql = new MySqlCommand(insertCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void produceAvenant()
    {
        //Recuperate the list of selected tiles - from DB
        try
        {
            //Create Avenant table
            Connect_DB();
            string avenantTable = "c" + clientId + "_p" + projectId + "_avenant";
            string deleteTable = "DROP TABLE IF EXISTS " + avenantTable + ";";
            string createTable = "CREATE TABLE IF NOT EXISTS " + avenantTable + " ( surface_id SMALLINT UNSIGNED NOT NULL, level TINYINT, room VARCHAR(20), libelle VARCHAR(100), comment VARCHAR(200), PRIMARY KEY (surface_id) ) CHARACTER SET 'utf8' ENGINE=INNODB;";
            MySqlCommand createAvenantTable = new MySqlCommand(deleteTable + createTable, con);
            MySqlDataReader myReader = createAvenantTable.ExecuteReader();
            myReader.Close();

            //Make the join
            Connect_DB();
            string surfacesTable = "c" + clientId + "_p" + projectId + "_surfaces";
            string choicesTable = "c" + clientId + "_p" + projectId + "_choices";
            string commentsTable = "c" + clientId + "_p" + projectId + "_comments";
            
            string cmd = "INSERT INTO " + avenantTable + " SELECT " + surfacesTable + ".id_surface, " + surfacesTable + ".level, " + surfacesTable + ".room_name, " + tilesTable + ".libelle, " + commentsTable + ".comment";
            cmd = cmd + " FROM " + surfacesTable;
            cmd = cmd + " INNER JOIN " + choicesTable + " ON " + surfacesTable + ".id_surface = " + choicesTable + ".id_surface";
            cmd = cmd + " INNER JOIN " + tilesTable + " ON " + choicesTable + ".id_tile = " + tilesTable + ".id";
            cmd = cmd + " INNER JOIN " + commentsTable + " ON " + commentsTable + ".id_surface = " + surfacesTable + ".id_surface;";

            MySqlCommand joinTables = new MySqlCommand(cmd, con);
            myReader = joinTables.ExecuteReader();

            while (myReader.Read())
            {
                Debug.Log(myReader["level"]);
            }
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        
    }

    public void ValidatePreSelection()
    {
        //Save preselection into DB - establish connection
        try
        {
            Connect_DB();
            //For the moment, I automatically drop the table to avoid it continuously growing from test to test...
            MySqlCommand cmdSqlDropTable = new MySqlCommand("DROP TABLE IF EXISTS preselections;", con);
            MySqlDataReader myReader0 = cmdSqlDropTable.ExecuteReader();
            myReader0.Close();

            //Then recreate it
            MySqlCommand cmdSqlCreateTable = new MySqlCommand("CREATE TABLE IF NOT EXISTS preselections ( client_id MEDIUMINT UNSIGNED NOT NULL, project_id MEDIUMINT UNSIGNED NOT NULL, tile_id MEDIUMINT UNSIGNED NOT NULL, PRIMARY KEY (client_id, project_id, tile_id)) CHARACTER SET 'utf8' ENGINE=INNODB;", con);
            MySqlDataReader myReader = cmdSqlCreateTable.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        //Save preselection into DB - part 1: save the DB
        MySqlCommand cmdSql = new MySqlCommand("INSERT INTO preselections VALUES ", con);
        MySqlCommand getTileIdCmd = new MySqlCommand("", con);
        int count = 0;
        foreach (string tile in GameObject.Find("PreselectionMenu").GetComponent<PreselectionMenuScript>().selectedTiles)
        {
            string tileId = "-1";
            if (count > 0)
            {
                cmdSql.CommandText += ",";
            }

            getTileIdCmd.CommandText = "SELECT id FROM " + tilesTable + " WHERE libelle='" + tile + "';";
            try
            {
                Connect_DB();
                MySqlDataReader myReader = getTileIdCmd.ExecuteReader();
                while (myReader.Read())
                    tileId = myReader["id"].ToString();
                myReader.Close();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
            cmdSql.CommandText = cmdSql.CommandText + "( " + clientId + ", " + projectId + ", " + tileId + ")";
            count += 1;
        }
        cmdSql.CommandText += ";";
        try
        {
            Connect_DB();
            int affectedRows = cmdSql.ExecuteNonQuery();
            Debug.Log(affectedRows + " affected rows");
            con.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }

        //Hide preselection menu
        GameObject.Find("PreselectionMenu").SetActive(false);

        //Reactivate player camera rotation
        GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>().cameraCanMove = true;
    }

    public void saveComment(string comment, GameObject surface)
    {
        //Create the table
        string createCmd = "CREATE TABLE IF NOT EXISTS c" + clientId + "_p" + projectId + "_comments ( id_surface SMALLINT UNSIGNED NOT NULL, comment VARCHAR(200), PRIMARY KEY (id_surface) ) CHARACTER SET 'utf8' ENGINE=INNODB;";
        try
        {
            MySqlCommand cmdSql = new MySqlCommand(createCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        //Insert comment
        string insertCmd = "REPLACE INTO c" + clientId + "_p" + projectId + "_comments (id_surface, comment) VALUES ( '" + surface.GetComponent<dummyMetadataScript>().ID + "', '" + comment + "');";
        try
        {
            MySqlCommand cmdSql = new MySqlCommand(insertCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    */
}
