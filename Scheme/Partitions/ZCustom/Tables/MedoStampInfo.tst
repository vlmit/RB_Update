<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="87e0def4-c862-4a34-aaa0-c6e896200d19" Name="MedoStampInfo" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="87e0def4-c862-0034-2000-06e896200d19" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="87e0def4-c862-0134-4000-06e896200d19" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="87e0def4-c862-0034-3100-06e896200d19" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="8de93a11-ea0d-46c0-9307-01f4db82011e" Name="X" Type="Int32 Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="6e002557-7d5f-4dc3-b15b-777c303e9f3e" Name="df_MedoStampInfo_X" Value="90" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="a1844c24-f373-4aab-bc0d-6e4907f0af63" Name="Y" Type="Int32 Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="cbd5affc-f693-4508-a8e1-4c032752092d" Name="df_MedoStampInfo_Y" Value="225" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="ff411025-4463-4000-a333-ce62384d9995" Name="Width" Type="Int32 Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="57dbeb03-9903-4452-8289-e6babd6384fc" Name="df_MedoStampInfo_Width" Value="60" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="1ba93d54-d58e-416d-987a-613e401cc2a3" Name="Height" Type="Int32 Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="433fd94d-a6d8-4ecc-9404-94e4613384f0" Name="df_MedoStampInfo_Height" Value="30" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="66c1fc08-da07-4648-879e-a9eafb4e6412" Name="Page" Type="Int32 Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="1c8a76da-d124-486b-9e79-ef957d761900" Name="df_MedoStampInfo_Page" Value="1" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="4a4055c1-cb11-462a-9678-d5e4b56fcafd" Name="typeID" Type="Int32 Null" />
	<SchemeComplexColumn ID="8dccfe99-226c-46f5-bb86-02e746c1a7e1" Name="StampType" Type="Reference(Typified) Not Null" ReferencedTable="307d2356-4fbe-41c4-b114-3497f2ac64ef" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="8dccfe99-226c-00f5-4000-02e746c1a7e1" Name="StampTypeID" Type="Int32 Not Null" ReferencedColumn="cac5c352-662d-4547-9f79-e77306a827e5">
			<SchemeDefaultConstraint IsPermanent="true" ID="6985bde4-e345-4c8e-b268-cfc44ee7148f" Name="df_MedoStampInfo_StampTypeID" Value="1" />
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="e4fc124d-9063-4be3-a145-3d7e353d30ab" Name="StampTypeName" Type="String(128) Null" ReferencedColumn="e893f1b3-1548-4d5e-ae40-286478ce6208">
			<SchemeDefaultConstraint IsPermanent="true" ID="2a984dcd-9e78-41e9-88f6-1a838ea77d5e" Name="df_MedoStampInfo_StampTypeName" Value="Штамп подписи" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="3c79be4f-63c2-4480-8e37-cdf05161d718" Name="FileSignatures" Type="Reference(Typified) Null" ReferencedTable="5f428478-eaf5-4180-bde9-499483c3f80c" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="3c79be4f-63c2-0080-4000-0df05161d718" Name="FileSignaturesRowID" Type="Guid Null" ReferencedColumn="5f428478-eaf5-0080-3100-099483c3f80c" />
		<SchemePhysicalColumn ID="942918bf-dab2-49b1-a0de-2a073db70665" Name="FileSignaturesUserName" Type="String(256) Null">
			<Description>Имя подписанта</Description>
		</SchemePhysicalColumn>
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="87e0def4-c862-0034-5000-06e896200d19" Name="pk_MedoStampInfo">
		<SchemeIndexedColumn Column="87e0def4-c862-0034-3100-06e896200d19" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="87e0def4-c862-0034-7000-06e896200d19" Name="idx_MedoStampInfo_ID" IsClustered="true">
		<SchemeIndexedColumn Column="87e0def4-c862-0134-4000-06e896200d19" />
	</SchemeIndex>
</SchemeTable>