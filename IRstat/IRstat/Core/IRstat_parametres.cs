namespace IRStat
{
    static class IRstat_parametres
    {
        public readonly static string xmlPlainFilenameDoesNotContains = "metadata";
        public readonly static string xmlMetaFileNameEndsWith = ".xml.metadata.properties.xml";

        public readonly static string openFolderDescription = "Choisir le dossier";

        public readonly static string XpathIrNom = @"/ead/eadheader/eadid";
        public readonly static string XpathMetaNom = @"/properties/entry[@key='sia:identifiant']";
        public readonly static string XpathMetaTitre = @"/properties/entry[@key='cm:title']";


        public readonly static int pageSize = 250;
        public readonly static int pageTimeout = 1000;
        //max XPathEngine memory (in MegaBytes)
        public readonly static long maxXPathEngineMemory = 1024;
    }
}