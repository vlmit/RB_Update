<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a" Partition="d1b372f3-7565-4309-9037-5e5a0969d94e">
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="5f76fb3c-5691-45a5-b8ad-5c9baf14ae61" Name="City" Type="String(Max) Null">
		<Description>Город</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="42210164-03ca-4254-8941-e4de2798652d" Name="Region" Type="String(Max) Null">
		<Description>Республика, область или край</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="83f21b4d-1c02-47f8-b21c-1c58fa0e538e" Name="Index" Type="String(Max) Null">
		<Description>Индекс</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="73f4b18a-57fe-4a10-a5fd-78c60b4f397c" Name="Country" Type="String(Max) Null">
		<Description>Страна или регион</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b1ab3fa2-29c4-4b71-bbc5-905d5ae84f71" Name="Web_page" Type="String(Max) Null">
		<Description>Web page</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="543a441f-3b02-426a-a4e4-ca06994804c9" Name="PartnerCategory" Type="Reference(Typified) Null" ReferencedTable="e8d856c2-3f2d-40d9-97f7-f5ee08b65301">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="543a441f-3b02-006a-4000-0a06994804c9" Name="PartnerCategoryID" Type="Guid Null" ReferencedColumn="e8d856c2-3f2d-01d9-4000-05ee08b65301" />
		<SchemeReferencingColumn ID="e91dd2b1-124b-497f-98cc-aff22746cd08" Name="PartnerCategoryName" Type="String(128) Null" ReferencedColumn="a3668609-e274-40d5-bf62-3c1cc57ad793" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f2be1f97-4211-48db-83df-8f208e5a84d3" Name="ExternalID" Type="String(128) Null">
		<Description>Поле для указания ID из SP по контрагенту</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="366fd5e8-568e-49d7-b7e6-6875428b2505" Name="MEDOFormat" Type="Reference(Typified) Null" ReferencedTable="533ab008-ae97-4d2c-9148-fe3c11f98a05">
		<Description>Формат МЭДО</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="366fd5e8-568e-00d7-4000-0875428b2505" Name="MEDOFormatID" Type="Int16 Null" ReferencedColumn="398ecbff-7e39-4e67-a7ad-d1ac1e0c4215" />
		<SchemeReferencingColumn ID="03d4bcf8-e49a-4ac6-9183-fa3154794aaf" Name="MEDOFormatFormat" Type="String(Max) Null" ReferencedColumn="3203c29c-b404-485c-82d3-542c1532b572" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d0e8b108-f66b-40a1-b011-45bece8a4c53" Name="MEDODSP" Type="Reference(Typified) Null" ReferencedTable="bba5210d-6c55-4ff0-9377-360827273371">
		<Description>ДСП МЭДО</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d0e8b108-f66b-00a1-4000-05bece8a4c53" Name="MEDODSPID" Type="Int16 Null" ReferencedColumn="7cab56e9-0c6b-47f9-8b33-3b1432d7ccdc" />
		<SchemeReferencingColumn ID="9ff304ec-f99b-4254-a47f-69ea9a407d2b" Name="MEDODSPDSP" Type="String(Max) Null" ReferencedColumn="df03da73-75bc-4115-8b81-38b897a0392e" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="2bfcb0d8-46fa-46ad-9236-92f188a007d1" Name="MEDOSecondaryID" Type="String(Max) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="ada99c8c-a206-4404-bc93-8bbfe5842b6c" Name="SortingWt" Type="Int32 Null">
		<Description>Вес для сортировки корреспондентов</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="65b63848-e168-497e-ac68-1455c278ef15" Name="MEDOActive" Type="Reference(Typified) Null" ReferencedTable="4d654422-f61b-4d84-8de7-527ec119a244">
		<Description>Статус МЭДО</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="65b63848-e168-007e-4000-0455c278ef15" Name="MEDOActiveID" Type="Int16 Null" ReferencedColumn="e08582a4-d97d-4c82-91cf-0ad9e38937e8" />
		<SchemeReferencingColumn ID="88c3e420-251f-4d81-af61-1d9e5dfe796e" Name="MEDOActiveActiveStatus" Type="String(Max) Null" ReferencedColumn="5680537b-d8db-43f9-9b77-e1a62f7f8f23" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="745788ea-1a23-472c-9eb0-c48d2dfea546" Name="MedoID" Type="String(Max) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="e8deda18-75f7-4b6f-8ead-bd8ed17a7460" Name="MedoAddress" Type="String(Max) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="9e54a763-5e21-406d-92e0-8ec2a8374829" Name="MEDOisActive" Type="Boolean Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a4a82a13-46b8-4da6-84aa-0257fcebd6f1" Name="MEDOisSecure" Type="Boolean Null" />
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="0f3ca02c-f1bb-4ccc-9ad5-a128a042f500" Name="GatewayOwner" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="0f3ca02c-f1bb-00cc-4000-0128a042f500" Name="GatewayOwnerID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="b3892956-fd90-4e0d-8721-63fd34b40288" Name="GatewayOwnerName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
		<SchemeReferencingColumn ID="5b4b5902-3e18-4570-8f50-972af3c02a98" Name="GatewayOwnerMedoID" Type="String(Max) Null" ReferencedColumn="745788ea-1a23-472c-9eb0-c48d2dfea546" />
		<SchemeReferencingColumn ID="781494be-3d50-4cee-86bf-f2da1ee48d37" Name="GatewayOwnerMedoAddress" Type="String(Max) Null" ReferencedColumn="e8deda18-75f7-4b6f-8ead-bd8ed17a7460" />
	</SchemeComplexColumn>
</SchemeTable>