import { ISignedData, SignatureProfile, SignatureType } from "tessa/cards";
import { FileContainer, CertData } from "tessa/files";
import { ValidationResultBuilder, ValidationResultType, IValidationResultBuilder } from "tessa/platform/validation";

export class ODFileSigningHelper {
    public static async signFileAsync(
    certNumber: string,
    fileContainer: FileContainer,
    fileID: string,
    validationResult: ValidationResultBuilder
  ): Promise<void> {
    let certificate = await this.getCertificateAsync(certNumber, validationResult);
    if (certificate == null) {
      validationResult.add(ValidationResultType.Error, 'Не удалось получить сертификат.');
    }

    const file = fileContainer.files.find(f => f.id == fileID)!;
    const contentDownloaded = await file.lastVersion.ensureContentDownloaded();
    if (!contentDownloaded || contentDownloaded.hasErrors) {
      return;
    }

    const content = file.lastVersion.content;
    if (!content) {
      validationResult.add(ValidationResultType.Error, 'Не удалось получить контент файла.');
      return;
    }

    const fileReader = new FileReader();
    const header = ';base64,';
    let data: string;
    try {
      data = (await this.readUploadedAsDataURLAsync(content!, fileReader)) as string;
    } catch (exception) {
      validationResult.add(ValidationResultType.Error, exception.message);
      return;
    }
    const sBase64Data = data!.substr(data!.indexOf(header) + header.length);

    const cadesplugin = window['cadesplugin'];
    const CPSigner = await cadesplugin.CreateObjectAsync('CAdESCOM.CPSigner');
    if (!CPSigner) {
      return;
    }
    await CPSigner.propset_Certificate(certificate);
    const CadesSignedData = await cadesplugin.CreateObjectAsync('CAdESCOM.CadesSignedData');
    if (!CadesSignedData) {
      return;
    }
    await CadesSignedData.propset_ContentEncoding(cadesplugin.CADESCOM_BASE64_TO_BINARY);
    await CadesSignedData.propset_Content(sBase64Data);
    const CADES_BES = 1;
    let signEDS: string;
    try {
      signEDS = await CadesSignedData.SignCades(CPSigner, CADES_BES, true);
    } catch (exception) {
      validationResult.add(ValidationResultType.Error, exception.message);
      return;
    }

    const signData: ISignedData = {
      signature: signEDS,
      profile: SignatureProfile.BES,
      type: SignatureType.CAdES
    };

    await fileContainer.addSignature(file.lastVersion,
      certificate! as CertData,
      signData,
      undefined,
      true
    );
  }

  public static async readUploadedAsDataURLAsync(file: File, fileReader: FileReader) {
    return new Promise((resolve, reject) => {
      fileReader.onerror = () => {
        fileReader.abort();
        reject(new DOMException());
      };

      fileReader.onload = () => {
        resolve(fileReader.result);
      };
      fileReader.readAsDataURL(file);
    });
  }

  private static async getCertificateAsync(
    certNumber: string,
    validationResult: IValidationResultBuilder
  ): Promise<any> {
    const cadesplugin = window['cadesplugin'];
    const store = await cadesplugin!.CreateObjectAsync('CAdESCOM.store');
    if (!store) {
      return;
    }

    try {
      await store.Open();
    } catch (exception) {
      validationResult.add(ValidationResultType.Error, exception);
      return;
    }

    const certificates = await store.Certificates;
    const certificatesCount = await certificates.Count;
    if (certificatesCount === 0) {
      store.Close();
      validationResult.add(ValidationResultType.Error, 'Не удалось найти сертификаты.');
      return;
    }

    for (let i = 1; i <= certificatesCount; i++) {
      let cert;
      cert = await certificates.Item(i);

      const serialNumber = await this.getCertificateSerialNumberAsync(cert);
      if (serialNumber == certNumber || serialNumber == '14789552fe3e42a743ec7fbd68520db9'
      ) {
        store.Close();
        return cert;
      }
    }
  }
  
  private static async getCertificateSerialNumberAsync(
    certificate: any
  ): Promise<string | null> {
    return await certificate.SerialNumber;
  }
}