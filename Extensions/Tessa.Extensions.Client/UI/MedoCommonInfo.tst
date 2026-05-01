<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="32d21922-5660-4c69-a65a-e500b20cf7ba" ID="042c178a-f22b-4285-b197-ba48f54e9d6f" Name="MedoCommonInfo" Group="Rosteh" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="042c178a-f22b-0085-2000-0a48f54e9d6f" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="042c178a-f22b-0185-4000-0a48f54e9d6f" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="042c178a-f22b-0085-3100-0a48f54e9d6f" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="86f5b77a-1c00-45a5-b23f-e0057877b845" Name="MessageID" Type="Guid Null">
		<Description>id сообщения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="a307de6c-247e-42ea-9679-a8125c461839" Name="ResponseMesID" Type="Guid Null">
		<Description>id ответного сообщения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="19dfff70-63ab-4a8a-b50e-74a10bab9790" Name="DateMessage" Type="DateTime Null">
		<Description>дата сообщения</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="99b48fb6-a3f7-42c8-a5a1-abc490006e0f" Name="MedoStatus" Type="Reference(Typified) Null" ReferencedTable="8e15b02a-c095-4db3-91d3-2f6c0186edca">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="99b48fb6-a3f7-00c8-4000-0bc490006e0f" Name="MedoStatusID" Type="Int32 Null" ReferencedColumn="edab72f6-36e3-4455-a4c3-5fe0d3ac81f3" />
		<SchemeReferencingColumn ID="18d03cdb-9357-403c-86b6-510f2743a79f" Name="MedoStatusName" Type="String(100) Null" ReferencedColumn="51e33100-dd63-4628-9521-9307b5ac2b8b" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="ab4465a8-5fbd-4a36-8b99-90e27786a109" Name="MedoType" Type="Reference(Typified) Null" ReferencedTable="30e42db8-7b78-4793-a391-b8b475a7c98c">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="ab4465a8-5fbd-0036-4000-00e27786a109" Name="MedoTypeID" Type="Int32 Null" ReferencedColumn="0110696c-7c6c-4a05-bb7a-7b7df2902aab" />
		<SchemeReferencingColumn ID="24c7e26a-8461-4ff1-81b9-47d3588a4a2b" Name="MedoTypeName" Type="String(128) Null" ReferencedColumn="cc5d5344-4c4e-42c5-b181-316dc617ba48" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="4dd4a4f6-0a88-42f1-bc36-92624b1b93f4" Name="MedoComment" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="ca6fd240-fcdf-4ce6-99a5-4602941f542b" Name="MedoError" Type="String(Max) Null" />
	<SchemeComplexColumn ID="18d89a7d-8d21-4bfe-99d6-3fcdede2f4a6" Name="MedoPartner" Type="Reference(Typified) Null" ReferencedTable="124a0efe-d43f-4723-9b09-602d44617fdc">
		<Description>контрагент мэдо</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="18d89a7d-8d21-00fe-4000-0fcdede2f4a6" Name="MedoPartnerID" Type="Guid Null" ReferencedColumn="124a0efe-d43f-0123-4000-002d44617fdc" />
		<SchemeReferencingColumn ID="7ccbe6ca-419a-4d30-9d78-3481fe3781eb" Name="MedoPartnerFullName" Type="String(1000) Null" ReferencedColumn="73afd968-eed6-4887-9fef-452f0bd0fda9" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="ab767778-75bf-4cd8-ae5c-b1d25c9077a3" Name="MedoXsdVersion" Type="Int32 Null">
		<Description>0 - 2.7, 1- 2.7.1</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="942cef28-923d-4b5d-a50d-8272c95ee4fb" Name="df_MedoCommonInfo_MedoXsdVersion" Value="1" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="bbd1cbf4-3faa-4aba-8e62-c5130f347680" Name="Person" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="bbd1cbf4-3faa-00ba-4000-05130f347680" Name="PersonID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="e371851d-e62b-4795-b8d4-88d4df5f8523" Name="PersonFullName" Type="String(256) Null" ReferencedColumn="e89b6dc3-7932-4d74-a99f-91b402029536" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="af8960d5-62c4-4abc-97dd-382f3692eff2" Name="MedoDocID" Type="Guid Null">
		<Description>GUID документа в системе отправителя</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="042c178a-f22b-0085-5000-0a48f54e9d6f" Name="pk_MedoCommonInfo">
		<SchemeIndexedColumn Column="042c178a-f22b-0085-3100-0a48f54e9d6f" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="042c178a-f22b-0085-7000-0a48f54e9d6f" Name="idx_MedoCommonInfo_ID" IsClustered="true">
		<SchemeIndexedColumn Column="042c178a-f22b-0185-4000-0a48f54e9d6f" />
	</SchemeIndex>
</SchemeTable>