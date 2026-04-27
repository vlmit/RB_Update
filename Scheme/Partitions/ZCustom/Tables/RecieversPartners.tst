<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b9979064-c267-4ea4-806b-e5147936a992" Name="RecieversPartners" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="b9979064-c267-00a4-2000-05147936a992" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="b9979064-c267-01a4-4000-05147936a992" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="b9979064-c267-00a4-3100-05147936a992" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="fbce0da5-71f3-4ef6-928e-d4c3af4cc181" Name="PartnerMedoAddress" Type="String(512) Null" />
	<SchemeComplexColumn ID="96597ef5-9edc-4c19-b3f7-eb0a00a05c3c" Name="DeliveryType" Type="Reference(Typified) Null" ReferencedTable="366fa575-2ada-446b-afa1-a68eca0ab8db">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="96597ef5-9edc-0019-4000-0b0a00a05c3c" Name="DeliveryTypeID" Type="Guid Null" ReferencedColumn="366fa575-2ada-016b-4000-068eca0ab8db" />
		<SchemeReferencingColumn ID="09671e1c-cf7b-43e6-8f34-241c5d7cb3ac" Name="DeliveryTypeName" Type="String(128) Null" ReferencedColumn="d1d10a19-8cb9-42f2-b242-69ae0e8f2ecd" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="692c7f7d-59cf-4f5c-92dd-a0789c4684a6" Name="Partner" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="692c7f7d-59cf-005c-4000-00789c4684a6" Name="PartnerID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="68765b1a-a43b-40a5-b6f8-60137382a5d9" Name="PartnerName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
		<SchemeReferencingColumn ID="4a4fe8f9-5004-45a5-932e-99fa9fd8352e" Name="PartnerHead" Type="String(256) Null" ReferencedColumn="309ff2b8-0f56-44f8-ae70-81144bc61605" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="b9979064-c267-00a4-5000-05147936a992" Name="pk_RecieversPartners">
		<SchemeIndexedColumn Column="b9979064-c267-00a4-3100-05147936a992" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="b9979064-c267-00a4-7000-05147936a992" Name="idx_RecieversPartners_ID" IsClustered="true">
		<SchemeIndexedColumn Column="b9979064-c267-01a4-4000-05147936a992" />
	</SchemeIndex>
</SchemeTable>